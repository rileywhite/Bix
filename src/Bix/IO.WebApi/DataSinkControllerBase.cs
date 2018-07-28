/***************************************************************************/
// Copyright 2013-2018 Riley White
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
/***************************************************************************/

using Bix.WebApi.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Bix.IO.WebApi
{
    [DisableRequestSizeLimit]
    public abstract class DataSinkControllerBase : BixControllerBase
    {
        protected IHttpContextAccessor HttpContextAccessor { get; }

        public DataSinkControllerBase(IHttpContextAccessor httpContextAccessor)
        {
            Contract.Requires(httpContextAccessor != null);
            Contract.Ensures(this.HttpContextAccessor != null);

            this.HttpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Bumps, or signals creation/completion/status update request, based on the stream's current state.
        /// </summary>
        /// <param name="streamStatus">Stream status with the source data filled in with info on the stream being signaled</param>
        /// <param name="cancellationToken">Used to cancel the operation</param>
        /// <returns>Stream status with target details filled in.</returns>
        /// <remarks>
        /// A returned value with equivalent target and sources hashes and a segment length equal to the full data length,
        /// then a completion signal was sent. In this case, <see cref="OnUploadCompleted( string,string)"/> will have been called.
        /// Otherwise, data uploading may begin/proceed if the caller has enough info, or the caller may bump again with a more
        /// targeted segment length.
        /// </remarks>
        [HttpPatch]
        public async Task<IActionResult> Bump([FromBody] StreamStatus streamStatus, CancellationToken cancellationToken)
        {
            var segmentStart = streamStatus.SegmentHashes[0].Start;
            var segmentLength = streamStatus.SegmentHashes.Sum(sh => sh.Length);

            var partition = this.HttpContextAccessor.HttpContext?.User?.Identity?.Name;

            var targetFilePath = GetTargetFilePath(partition, streamStatus.Descriptor.Id);

            if (!Directory.Exists(Path.GetDirectoryName(targetFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath));
            }

            using (var targetStream = new FileStream(targetFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                switch (targetStream.Length)
                {
                    case long l when l < streamStatus.Descriptor.Length:
                        // more efficient than SetLength since the file isn't filled with 0s
                        targetStream.Position = streamStatus.Descriptor.Length - 1;
                        targetStream.WriteByte(0);
                        targetStream.Position = 0;
                        break;

                    case long l when l > streamStatus.Descriptor.Length:
                        // truncate the file
                        targetStream.SetLength(streamStatus.Descriptor.Length);
                        break;
                }
            }

            var targetFile = new FileInfo(targetFilePath);

            var hasher = new SegmentHasher();
            if (hasher.TryGetSubsegmentHashesOfFirstDiff(
                streamStatus.SegmentHashes,
                targetFile,
                segmentStart,
                segmentLength,
                (byte)streamStatus.SegmentHashes.Length,
                new HashAlgorithmName(streamStatus.Descriptor.HashName),
                out var diffSubsegmentHashes))
            {
                streamStatus.SegmentHashes = diffSubsegmentHashes;
                return this.Ok(streamStatus);
            }

            // no diff
            // check if we were comparing the full file, and if so, indicate file upload completeness
            if (segmentStart == 0 && segmentLength == targetFile.Length)
            {
                await this.SignalUploadCompleted(partition, streamStatus.Descriptor.Id, targetFile);
            }

            // TODO null segment hashes is a poor indicator of a lack of difference
            //      and an even poorer indicator that that the OnUploadCompleted event was raised
            streamStatus.SegmentHashes = null;
            return this.Ok(streamStatus);
        }

        private static string GetTargetFilePath(string partition, string id)
        {
            if (string.IsNullOrWhiteSpace(partition)) { partition = "DefaultPartition"; };

            // replace any invalid partition characters with underscores
            // (potential for collision here, but I'm accepting that risk until a need to change it emerges)
            partition = string.Join("_", partition.Split(Path.GetInvalidFileNameChars()));

            return Path.Combine(
                Path.GetTempPath(),
                "BixDataUpload",
                partition,
                id);
        }

        /// <summary>
        /// Buffer size for reading from streaming input and also for writing to file.
        /// </summary>
        public virtual int IOBufferSize { get; } = 81920;

        /// <summary>
        /// Accepts a stream representing the data to transfer. Calls <see cref="OnUploadCompleted( string,string)"/> on success.
        /// </summary>
        /// <param name="id">Identifier for the upload. Must be unique for an authenticated user within the timeframe of the upload.</param>
        /// <param name="startAt">Position within the target data where writing should start. Defaults to 0, which indicates that a full set of data is being transferred.</param>
        /// <param name="cancellationToken">Used to cancel the operation</param>
        /// <returns>Action result</returns>
        /// <remarks>
        /// Regardless of the value of <paramref name="startAt"/>, the source data is read from the beginning. This means that
        /// if a transfer is being restarted, the client should send a stream beginning at the restart point. The server side
        /// will begin writing at the correct location in the target stream.
        /// </remarks>
        [HttpPatch("{id}")]
        [HttpPatch("{id}/{startAt}")]
        public async Task<IActionResult> SendData(string id, long startAt = 0, CancellationToken cancellationToken = default)
        {
            var partition = this.HttpContextAccessor.HttpContext?.User?.Identity?.Name;
            var targetFilePath = GetTargetFilePath(partition, id);
            var targetFile = new FileInfo(targetFilePath);

            using (var memoryMappedFile = MemoryMappedFile.CreateFromFile(targetFile.FullName))
            using (var targetStream = memoryMappedFile.CreateViewStream(startAt, targetFile.Length - startAt, MemoryMappedFileAccess.Write))
            {
                await this.Request.Body.CopyToAsync(targetStream, this.IOBufferSize, cancellationToken);
            }

            await this.SignalUploadCompleted(partition, id, targetFile);

            return this.Ok();
        }

        /// <summary>
        /// Wraps call to <see cref="OnUploadCompleted(string, string, FileInfo)"/>. Will try to clean up the temp uploaded file
        /// after a successful completion of the upload completion method.
        /// </summary>
        /// <param name="partition">Partition that separates sets of unique IDs for </param>
        /// <param name="id">Identifier for the upload. Must be unique for an authenticated user within the timeframe of the upload.</param>
        /// <param name="tempFileInfo">FileInfo for the temporary location of the uploaded data. Overridden method should clean up/move this file. If it does not, then this base type will attempt to.</param>
        /// <returns>Async task</returns>
        private async Task SignalUploadCompleted(string partition, string id, FileInfo tempFileInfo)
        {
            await this.OnUploadCompleted(partition, id, tempFileInfo);
            if (tempFileInfo.Exists) { try { tempFileInfo.Delete(); } catch { /* Ignore */ } }
        }

        /// <summary>
        /// When overridden in a subclass, should process the uploaded data, including moving/deleting the file.
        /// </summary>
        /// <param name="partition">Partition that separates sets of unique IDs for </param>
        /// <param name="id">Identifier for the upload. Must be unique for an authenticated user within the timeframe of the upload.</param>
        /// <param name="tempFileInfo">FileInfo for the temporary location of the uploaded data. Overridden method should clean up/move this file. If it does not, then this base type will attempt to.</param>
        /// <returns>Async task</returns>
        /// <remarks>The possibility exists of multiple calls, so implementations should be idempotent.</remarks>
        protected abstract Task OnUploadCompleted(string partition, string id, FileInfo tempFileInfo);
    }
}
