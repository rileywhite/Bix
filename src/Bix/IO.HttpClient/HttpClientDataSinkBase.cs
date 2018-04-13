﻿/***************************************************************************/
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

using Bix.Core;
using Bix.Http.Client;
using Bix.Http.Core;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using snh = System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Bix.IO.HttpClient
{
    public abstract class HttpClientDataSinkBase
    {
        protected IHttpClientConfiguration Config { get; }
        protected IAuthenticationHeaderGenerator AuthenticationHeaderGenerator { get; }
        protected ILogger Logger { get; }

        protected abstract string DataSinkControllerPath { get; }

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
        public async Task<StreamStatus> Bump(StreamStatus streamStatus, CancellationToken cancellationToken)
        {
            try
            {
                using (var client = new snh.HttpClient())
                {
                    var response = await client.PatchWithAuthenticationAsync(
                        $"{this.Config.BaseControllerPath}{this.DataSinkControllerPath}",
                        streamStatus.ToJson(),
                        this.AuthenticationHeaderGenerator,
                        cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new DataSinkHttpClientException("Unsuccessful service call response") { ErrorResponse = response };
                    }
                    using (var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync()))
                    using (var jsonTextReader = new JsonTextReader(streamReader))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        return new JsonSerializer().Deserialize<StreamStatus>(jsonTextReader);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, "Failure to GetAllAsync");
                throw;
            }
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
        public async Task SendFullData(string id, Stream stream, CancellationToken cancellationToken)
        {
        }

        /// <summary>
        /// Accepts a stream representing the remaining amount of data to upload to continue a detected incomplete upload. Calls <see cref="OnUploadCompleted( string,string)"/> on success.
        /// </summary>
        /// <param name="id">Identifier for the upload. Must be unique for an authenticated user within the timeframe of the upload.</param>
        /// <param name="startAt">Position within the target data where writing should start.</param>
        /// <param name="stream"><see cref="Stream"/> for accessing the remaining data. The first byte in the stream will be writtent at <paramref name="startAt"/>, and the rest will be written in order.</param>
        /// <param name="cancellationToken">Used to cancel the operation</param>
        /// <returns>Action result</returns>
        public async Task SendRemainingData(string id, long startAt, Stream stream, CancellationToken cancellationToken)
        {
        }

        /// <summary>
        /// Accepts a stream representing a sub-segment of data to upload. Never calls <see cref="OnUploadCompleted( string,string)"/>, even on success, as no order of uploaded segments is assumed.
        /// </summary>
        /// <param name="id">Identifier for the upload. Must be unique for an authenticated user within the timeframe of the upload.</param>
        /// <param name="startAt"></param>
        /// <param name="stream"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Action result</returns>
        public async Task SendPartialData(string id, long startAt, Stream stream, CancellationToken cancellationToken)
        {
        }
    }
}