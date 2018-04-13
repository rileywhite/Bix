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

using Bix.Core;
using Bix.Http.Client;
using Bix.Http.Core;
using Bix.IO;
using Newtonsoft.Json;
using Serilog;
using System;
using System.IO;
using snh = System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Bix.Repositories.Restful.HttpClient
{
    public abstract class DataUploadClientBase
    {
        protected IHttpClientConfiguration Config { get; }
        protected IAuthenticationHeaderGenerator AuthenticationHeaderGenerator { get; }
        protected ILogger Logger { get; }

        protected abstract string UploadControllerPath { get; }

        public DataUploadClientBase(
            IHttpClientConfiguration config,
            IAuthenticationHeaderGenerator authenticationHeaderGenerator,
            ILogger logger)
        {
            this.Config = config;
            this.AuthenticationHeaderGenerator = authenticationHeaderGenerator;
            this.Logger = logger;
        }

        public async Task<StreamStatus> CreateOrGetStreamStatus(StreamStatus streamStatus, CancellationToken cancellationToken)
        {
            try
            {
                using (var client = new snh.HttpClient())
                {
                    var response = await client.PatchWithAuthenticationAsync(
                        $"{this.Config.BaseControllerPath}{this.UploadControllerPath}",
                        streamStatus.ToJson(),
                        this.AuthenticationHeaderGenerator,
                        cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new RestfulRepositoryHttpClientException("Unsuccessful service call response") { ErrorResponse = response };
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
                this.Logger.Error(ex, "Failure to CreateOrGetStreamStatus. StreamStatus {StreamStatus}", streamStatus.ToJson());
                throw;
            }
        }

        public async Task SendData(string id, long startAt, Stream stream, CancellationToken cancellationToken)
        {
            try
            {
                stream.Position = startAt;
                using (var client = new snh.HttpClient())
                {
                    var response = await client.PatchWithAuthenticationAsync(
                        $"{this.Config.BaseControllerPath}{this.UploadControllerPath}/{id}/{startAt}",
                        stream,
                        this.AuthenticationHeaderGenerator,
                        cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new RestfulRepositoryHttpClientException("Unsuccessful service call response") { ErrorResponse = response };
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, "Failure to SendData. Upload may be resumable depending on error. Stream ID {StreamId}", id);
                throw;
            }
        }
    }
}
