/***************************************************************************/
// Copyright 2013-2019 Riley White
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
using System.IO;
using snh = System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
using System.Net;

namespace Bix.IO.HttpClient
{
    /// <summary>
    /// Base type for sending large data over HTTP.
    /// </summary>
    /// <remarks>Currently only handles data with a known size.</remarks>
    public abstract class HttpClientDataSinkBase
    {
        protected IHttpClientConfiguration Config { get; }
        protected IAuthenticationHeaderGenerator AuthenticationHeaderGenerator { get; }
        protected ILogger Logger { get; }

        public HttpClientDataSinkBase(
            IHttpClientConfiguration config,
            IAuthenticationHeaderGenerator authenticationHeaderGenerator,
            ILogger logger)
        {
            Contract.Requires(config != null);
            Contract.Requires(authenticationHeaderGenerator != null);
            Contract.Requires(logger != null);
            Contract.Ensures(this.Config != null);
            Contract.Ensures(this.AuthenticationHeaderGenerator != null);
            Contract.Ensures(this.Logger != null);

            this.Config = config;
            this.AuthenticationHeaderGenerator = authenticationHeaderGenerator;
            this.Logger = logger;
        }

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
        public async Task<StreamStatus> BumpAsync(StreamStatus streamStatus, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var client = new snh.HttpClient())
                {
                    var response = await client.PatchWithAuthenticationAsync(
                        $"{this.Config.BaseControllerPath}{this.DataSinkControllerPath}",
                        streamStatus.ToJson(),
                        this.AuthenticationHeaderGenerator,
                        this.Logger,
                        cancellationToken).ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new DataSinkHttpClientException("Unsuccessful service call response") { ErrorResponse = response };
                    }
                    using (var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync().ConfigureAwait(false)))
                    using (var jsonTextReader = new JsonTextReader(streamReader))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        return new JsonSerializer().Deserialize<StreamStatus>(jsonTextReader);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, "Failure to BumpAsync");
                throw;
            }
        }

        /// <summary>
        /// Buffer size for reading from streaming input and also for writing to file.
        /// </summary>
        public virtual int IOBufferSize { get; } = 81920;

        /// <summary>
        /// Accepts a stream representing the remaining amount of data to upload to continue a detected incomplete upload. Calls <see cref="OnUploadCompleted( string,string)"/> on success.
        /// </summary>
        /// <param name="id">Identifier for the upload. Must be unique for an authenticated user within the timeframe of the upload.</param>
        /// <param name="stream"><see cref="Stream"/> for accessing the remaining data. The first byte in the stream will be written at <paramref name="startAt"/>, and the rest will be written in order.</param>
        /// <param name="startAt">Position within the target data where writing should start.</param>
        /// <param name="length">If provided, the given number of bytes will be streamed. Defaults to <c>null</c>, meaning that all bytes will be streamed. Either way, the completion action will be invoked only if the last expected data byte is sent (based on expected data length). If given for a partial data upload, then the service can significantly reduce memory usage.</param>
        /// <param name="cancellationToken">Used to cancel the operation</param>
        public async Task SendDataAsync(string id, Stream stream, long startAt = 0, long? length = default, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var client = new snh.HttpClient())
                {
                    client.Timeout = Timeout.InfiniteTimeSpan;

                    string requestUri;
                    if (length.HasValue)
                    {
                        requestUri = $"{this.Config.BaseControllerPath}{this.DataSinkControllerPath}/{id}/{startAt}/{length.Value}";
                    }
                    else if (startAt > 0)
                    {
                        requestUri = $"{this.Config.BaseControllerPath}{this.DataSinkControllerPath}/{id}/{startAt}";
                    }
                    else
                    {
                        requestUri = $"{this.Config.BaseControllerPath}{this.DataSinkControllerPath}/{id}";
                    }


                    var response = await client.PatchWithAuthenticationAsync(
                        requestUri,
                        stream,
                        "application/octet-stream",
                        this.IOBufferSize,
                        this.AuthenticationHeaderGenerator,
                        this.Logger,
                        cancellationToken).ConfigureAwait(false);

                    EnsureSuccessfulDataTransfer(response);
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, "Failure to SendDataAsync");
                throw;
            }
        }

        private static void EnsureSuccessfulDataTransfer(snh.HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new DataSinkHttpClientException("404 error response. If the controller exists and handles PATCH, then this may be caused by large message request filtering on the server, i.e. 404.13.") { ErrorResponse = response };
                }

                throw new DataSinkHttpClientException("Unsuccessful service call response") { ErrorResponse = response };
            }
        }
    }
}
