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
using System.Threading;
using System.Threading.Tasks;

namespace Bix.IO.WebApi
{
    public abstract class DataSinkControllerBase : BixControllerBase
    {
        protected IHttpContextAccessor HttpContextAccessor { get; }
        protected ITargetStreamFactory TargetStreamFactory { get; }

        public DataSinkControllerBase(IHttpContextAccessor httpContextAccessor, ITargetStreamFactory targetStreamFactory)
        {
            Contract.Requires(httpContextAccessor != null);
            Contract.Requires(targetStreamFactory != null);
            Contract.Ensures(this.HttpContextAccessor != null);
            Contract.Ensures(this.TargetStreamFactory != null);

            this.HttpContextAccessor = httpContextAccessor;
            this.TargetStreamFactory = targetStreamFactory;
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
            using (var targetStream = this.TargetStreamFactory.CreateStream(this.HttpContextAccessor.HttpContext?.User?.Identity?.Name, streamStatus.Descriptor.Id))
            {
                switch (targetStream.Length)
                {
                    case long l when l < streamStatus.Descriptor.Length:
                        // more efficient that SetLength since the file isn't filled with 0s
                        targetStream.Position = streamStatus.Descriptor.Length - 1;
                        targetStream.WriteByte(0);
                        break;

                    case long l when l > streamStatus.Descriptor.Length:
                        // truncate the file
                        targetStream.SetLength(streamStatus.Descriptor.Length);
                        break;
                }

                using (var hashChecker = new StreamMultipartHashChecker(targetStream))
                {
                    var sourceDetails = streamStatus.SourceSubstreamDetails;

                    streamStatus.TargetSubstreamDetails = await hashChecker.GetSubstreamDetails(
                        sourceDetails.Start,
                        sourceDetails.Length,
                        (byte)sourceDetails.SegmentHashes.Length,
                        sourceDetails.HashName);
                }
            }

            if (streamStatus.TargetSubstreamDetails.Length == streamStatus.Descriptor.Length &&
                streamStatus.TargetSubstreamDetails.HashName == streamStatus.Descriptor.HashName &&
                streamStatus.TargetSubstreamDetails.Hash == streamStatus.Descriptor.Hash)
            {
                await this.OnUploadCompleted(streamStatus.Descriptor.Id);
            }

            return this.Ok(streamStatus);
        }

        /// <summary>
        /// Buffer size for reading from streaming input and also for writing to file.
        /// </summary>
        protected virtual int IOBufferSizes { get; } = 81920;

        /// <summary>
        /// Accepts a stream representing the full amount of data to upload. Calls <see cref="OnUploadCompleted( string,string)"/> on success.
        /// </summary>
        /// <param name="id">Identifier for the upload. Must be unique for an authenticated user within the timeframe of the upload.</param>
        /// <param name="stream"><see cref="Stream"/> for accessing the full data, from beginning to end.</param>
        /// <param name="cancellationToken">Used to cancel the operation</param>
        /// <returns>Action result</returns>
        [HttpPatch("{id}")]
        public async Task<IActionResult> SendFullData(string id, [FromBody] Stream stream, CancellationToken cancellationToken)
        {

            using (var targetStream = this.TargetStreamFactory.CreateStream(this.HttpContextAccessor.HttpContext?.User?.Identity?.Name, id))
            {
                await stream.CopyToAsync(targetStream, this.IOBufferSizes, cancellationToken);
            }

            await this.OnUploadCompleted(id);

            return this.Ok();
        }

        /// <summary>
        /// Accepts a stream representing the remaining amount of data to upload to continue a detected incomplete upload. Calls <see cref="OnUploadCompleted( string,string)"/> on success.
        /// </summary>
        /// <param name="id">Identifier for the upload. Must be unique for an authenticated user within the timeframe of the upload.</param>
        /// <param name="startAt">Position within the target data where writing should start.</param>
        /// <param name="stream"><see cref="Stream"/> for accessing the remaining data. The first byte in the stream will be writtent at <paramref name="startAt"/>, and the rest will be written in order.</param>
        /// <param name="cancellationToken">Used to cancel the operation</param>
        /// <returns>Action result</returns>
        [HttpPatch("{id}/{startAt}")]
        public async Task<IActionResult> SendRemainingData(string id, long startAt, [FromBody] Stream stream, CancellationToken cancellationToken)
        {
            using (var targetStream = this.TargetStreamFactory.CreateStream(this.HttpContextAccessor.HttpContext?.User?.Identity?.Name, id))
            {
                targetStream.Position = startAt;
                await stream.CopyToAsync(targetStream, this.IOBufferSizes, cancellationToken);
            }

            await this.OnUploadCompleted(id);

            return this.Ok();
        }

        /// <summary>
        /// Accepts a stream representing a sub-segment of data to upload. Never calls <see cref="OnUploadCompleted( string,string)"/>, even on success, as no order of uploaded segments is assumed.
        /// </summary>
        /// <param name="id">Identifier for the upload. Must be unique for an authenticated user within the timeframe of the upload.</param>
        /// <param name="startAt"></param>
        /// <param name="stream"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Action result</returns>
        [HttpPatch("{id}/{startAt}/{fullDataLength}")]
        public async Task<IActionResult> SendPartialData(string id, long startAt, [FromBody] Stream stream, CancellationToken cancellationToken)
        {
            using (var targetStream = this.TargetStreamFactory.CreateStream(this.HttpContextAccessor.HttpContext?.User?.Identity?.Name, id))
            {
                targetStream.Position = startAt;
                await stream.CopyToAsync(targetStream, this.IOBufferSizes, cancellationToken);
            }

            return this.Ok();
        }

        /// <summary>
        /// When overridden in a subclass, should process the uploaded data, including moving/deleting the file.
        /// </summary>
        /// <param name="id">Identifier for the upload. Must be unique for an authenticated user within the timeframe of the upload.</param>
        /// <returns>Async task</returns>
        private async Task OnUploadCompleted(string id) { await this.OnUploadCompleted(this.HttpContextAccessor.HttpContext?.User?.Identity?.Name, id); }

        /// <summary>
        /// When overridden in a subclass, should process the uploaded data, including moving/deleting the file.
        /// </summary>
        /// <param name="partition">Partition that separates sets of unique IDs for </param>
        /// <param name="id">Identifier for the upload. Must be unique for an authenticated user within the timeframe of the upload.</param>
        /// <returns>Async task</returns>
        protected abstract Task OnUploadCompleted(string partition, string id);
    }
}
