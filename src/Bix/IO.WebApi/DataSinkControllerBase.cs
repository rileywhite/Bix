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
using Microsoft.Extensions.Logging;
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
    /// <summary>
    /// Base type for controllers that accept streaming data of a known size.
    /// </summary>
    [DisableRequestSizeLimit]
    public abstract class DataSinkControllerBase : BixControllerBase
    {
        /// <summary>
        /// Creates a new <see cref="DataSinkControllerBase"/>
        /// </summary>
        /// <param name="httpContextAccessor">For accessing info about the current HTTP request</param>
        /// <param name="logger">Logger to which log entries will be written</param>
        public DataSinkControllerBase(IHttpContextAccessor httpContextAccessor, ILogger logger)
        {
            Contract.Requires(httpContextAccessor != null);
            Contract.Requires(logger != null);
            Contract.Ensures(this.HttpContextAccessor != null);
            Contract.Ensures(this.Logger != null);

            this.HttpContextAccessor = httpContextAccessor;
            this.Logger = logger;
        }

        /// <summary>
        /// Gets the HTTP context accessor for the current request
        /// </summary>
        protected IHttpContextAccessor HttpContextAccessor { get; }

        /// <summary>
        /// Gets the current logger
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Bumps, or signals creation/completion/status update request, based on the stream's current state.
        /// </summary>
        /// <param name="streamStatus">Stream status with the source data filled in with info on the stream being signaled</param>
        /// <param name="cancellationToken">Used to cancel the operation</param>
        /// <returns>Stream status with target details filled in or <c>null</c> if the target stream already contains full data.</returns>
        /// <remarks>
        /// A returned value with equivalent target and sources hashes and a segment length equal to the full data length,
        /// then a completion signal was sent. In this case, <see cref="OnUploadCompleted(string, string, FileInfo, CancellationToken)"/> will have been called.
        /// Otherwise, data uploading may begin/proceed if the caller has enough info, or the caller may bump again with a more
        /// targeted segment length.
        /// </remarks>
        [HttpPatch]
        public async Task<IActionResult> Bump([FromBody] StreamStatus streamStatus, CancellationToken cancellationToken = default)
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
                        Logger.LogInformation($"Setting length of file {targetFilePath} with partition {partition} and ID {streamStatus.Descriptor.Id} to {streamStatus.Descriptor.Length}.");
                        targetStream.Position = streamStatus.Descriptor.Length - 1;
                        targetStream.WriteByte(0);
                        targetStream.Position = 0;
                        break;

                    case long l when l > streamStatus.Descriptor.Length:
                        // truncate the file
                        Logger.LogWarning($"Truncating length of file {targetFilePath} with partition {partition} and ID {streamStatus.Descriptor.Id} to {streamStatus.Descriptor.Length}.");
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
                Logger.LogDebug($"Difference was found for the data stream with partition {partition}, ID {streamStatus.Descriptor.Id}, segment start {segmentStart}, and segment length {segmentLength}.");
                streamStatus.SegmentHashes = diffSubsegmentHashes;
                return this.Ok(streamStatus);
            }

            // TODO null segment hashes is a poor indicator of a lack of difference
            //      and an even poorer indicator that that the OnUploadCompleted event was raised
            streamStatus.SegmentHashes = null;

            // no diff
            // check if we were comparing the full file, and if so, indicate file upload completeness
            if (segmentStart == 0 && segmentLength == targetFile.Length)
            {
                Logger.LogInformation($"No difference was found for a full data stream with partition {partition} and ID {streamStatus.Descriptor.Id}. Signaling upload completed.");
                await this.SignalUploadCompleted(partition, streamStatus.Descriptor.Id, targetFile, cancellationToken);
            }
            else
            {
                Logger.LogInformation($"No difference was found for a partial data stream with partition {partition}, ID {streamStatus.Descriptor.Id}, segment start {segmentStart}, and segment length {segmentLength}.");
            }

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
        /// Accepts a stream representing the data to transfer. Calls <see cref="OnUploadCompleted(string, string, FileInfo, CancellationToken)"/> on success.
        /// </summary>
        /// <param name="id">Identifier for the upload. Must be unique for an authenticated user within the timeframe of the upload.</param>
        /// <param name="startAt">Position within the target data where writing should start. Defaults to 0, which indicates that a full set of data is being transferred.</param>
        /// <param name="length">If provided, the given number of bytes will be streamed. Defaults to <c>null</c>, meaning that all bytes will be streamed. Either way, the completion action will be invoked only if the last expected data byte is sent (based on expected data length). If given for a partial data upload, then the service can significantly reduce memory usage.</param>
        /// <param name="cancellationToken">Used to cancel the operation</param>
        /// <returns>Action result</returns>
        /// <remarks>
        /// Regardless of the value of <paramref name="startAt"/>, the source data is read from the beginning. This means that
        /// if a transfer is being restarted, the client should send a stream beginning at the restart point. The server side
        /// will begin writing at the correct location in the target stream.
        /// </remarks>
        [HttpPatch("{id}")]
        [HttpPatch("{id}/{startAt}")]
        [HttpPatch("{id}/{startAt}/{length}")]
        public async Task<IActionResult> SendData(string id, long startAt = 0, long? length = default, CancellationToken cancellationToken = default)
        {
            var partition = this.HttpContextAccessor.HttpContext?.User?.Identity?.Name;
            var targetFilePath = GetTargetFilePath(partition, id);
            var targetFile = new FileInfo(targetFilePath);

            var bytesLeft = targetFile.Length - startAt;
            length = length ?? bytesLeft;
            if (length.Value > bytesLeft)
            {
                // too much data sent
                this.Logger.LogWarning($"Expected no more than {bytesLeft} bytes, but {length.Value} bytes were indicated.");
                return this.BadRequest();
            }

            Logger.LogInformation($"Starting to write {length.Value} expected bytes to {targetFilePath} with partition {partition} and ID {id}.");

            var actualWrittenBytes = 0L;

            using (var memoryMappedFile = MemoryMappedFile.CreateFromFile(targetFile.FullName))
            using (var targetStream = memoryMappedFile.CreateViewStream(startAt, length.Value, MemoryMappedFileAccess.Write))
            {
                await this.Request.Body.CopyToAsync(targetStream, this.IOBufferSize, cancellationToken);
                actualWrittenBytes = targetStream.Position;
            }

            if (actualWrittenBytes != length.Value)
            {
                Logger.LogWarning($"Wrote {length.Value} bytes to {targetFilePath} with partition {partition} and ID {id}. This was fewer than expected. Specify a length parameter to avoid allocating unneeded memory in the service.");
            }
            else
            {
                Logger.LogInformation($"Wrote {length.Value} bytes to {targetFilePath} with partition {partition} and ID {id}.");
            }

            if (actualWrittenBytes == bytesLeft)
            {
                // last of the data was sent
                Logger.LogInformation($"Signaling completion after data transfer for {targetFilePath} with partition {partition} and ID {id}.");
                await this.SignalUploadCompleted(partition, id, targetFile, cancellationToken);
            }

            return this.Ok();
        }

        /// <summary>
        /// Wraps call to <see cref="OnUploadCompleted(string, string, FileInfo, CancellationToken)"/>. Will try to clean up the temp uploaded file
        /// after a successful completion of the upload completion method.
        /// </summary>
        /// <param name="partition">Partition that separates sets of unique IDs for </param>
        /// <param name="id">Identifier for the upload. Must be unique for an authenticated user within the timeframe of the upload.</param>
        /// <param name="tempFileInfo">FileInfo for the temporary location of the uploaded data. Overridden method should clean up/move this file. If it does not, then this base type will attempt to.</param>
        /// <param name="cancellationToken">Used to cancel the operation</param>
        /// <returns>Async task</returns>
        private async Task SignalUploadCompleted(string partition, string id, FileInfo tempFileInfo, CancellationToken cancellationToken = default)
        {
            await this.OnUploadCompleted(partition, id, tempFileInfo, cancellationToken);
            if (tempFileInfo.Exists) { try { tempFileInfo.Delete(); } catch { /* Ignore */ } }
        }

        /// <summary>
        /// When overridden in a subclass, should process the uploaded data, including moving/deleting the file.
        /// </summary>
        /// <param name="partition">Partition that separates sets of unique IDs for </param>
        /// <param name="id">Identifier for the upload. Must be unique for an authenticated user within the timeframe of the upload.</param>
        /// <param name="tempFileInfo">FileInfo for the temporary location of the uploaded data. Overridden method should clean up/move this file. If it does not, then this base type will attempt to.</param>
        /// <param name="cancellationToken">Used to cancel the operation</param>
        /// <returns>Async task</returns>
        /// <remarks>The possibility exists of multiple calls, so implementations should be idempotent.</remarks>
        protected abstract Task OnUploadCompleted(string partition, string id, FileInfo tempFileInfo, CancellationToken cancellationToken = default);
    }
}
