/***************************************************************************/
// Copyright 2013-2022 Riley White
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

using Bix.Http.Core;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bix.Http.Client
{
    public static class HttpClientAuthExtensions
    {
        public static async Task<Stream> GetStreamWithAuthenticationAsync(
            this HttpClient client,
            string requestUri,
            IAuthenticationHeaderGenerator authenticationHeaderGenerator,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            client.DefaultRequestHeaders.Authorization =
                authenticationHeaderGenerator.GenerateAuthenticationHeader(requestUri);
            cancellationToken.ThrowIfCancellationRequested();
            logger?.LogDebug("Added auth header to {Verb} call to {RequestUri}", "GET", requestUri);
            return await client.GetStreamAsync(requestUri).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> PostWithAuthenticationAsync(
            this HttpClient client,
            string requestUri,
            string jsonContent,
            IAuthenticationHeaderGenerator authenticationHeaderGenerator,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            client.DefaultRequestHeaders.Authorization =
                authenticationHeaderGenerator.GenerateAuthenticationHeader(requestUri, jsonContent);
            logger?.LogDebug("Added auth header to {Verb} call to {RequestUri}", "POST", requestUri);
            return await client.PostAsync(requestUri, new StringContent(jsonContent, Encoding.UTF8, "application/json"), cancellationToken).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> DeleteWithAuthenticationAsync(
            this HttpClient client,
            string requestUri,
            IAuthenticationHeaderGenerator authenticationHeaderGenerator,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            client.DefaultRequestHeaders.Authorization =
                authenticationHeaderGenerator.GenerateAuthenticationHeader(requestUri);
            logger?.LogDebug("Added auth header to {Verb} call to {RequestUri}", "DELETE", requestUri);
            return await client.DeleteAsync(requestUri, cancellationToken).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> PutWithAuthenticationAsync(
            this HttpClient client,
            string requestUri,
            string jsonContent,
            IAuthenticationHeaderGenerator authenticationHeaderGenerator,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            client.DefaultRequestHeaders.Authorization =
                authenticationHeaderGenerator.GenerateAuthenticationHeader(requestUri, jsonContent);
            logger?.LogDebug("Added auth header to {Verb} call to {RequestUri}", "PUT", requestUri);
            return await client.PutAsync(requestUri, new StringContent(jsonContent, Encoding.UTF8, "application/json"), cancellationToken).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> PatchWithAuthenticationAsync(
            this HttpClient client,
            string requestUri,
            string jsonContent,
            IAuthenticationHeaderGenerator authenticationHeaderGenerator,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            client.DefaultRequestHeaders.Authorization =
                authenticationHeaderGenerator.GenerateAuthenticationHeader(requestUri, jsonContent);
            logger?.LogDebug("Added auth header to {Verb} call to {RequestUri}", "PATCH", requestUri);
            return await client.PatchAsync(requestUri, new StringContent(jsonContent, Encoding.UTF8, "application/json"), cancellationToken).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> PatchWithAuthenticationAsync(
            this HttpClient client,
            string requestUri,
            Stream contentDataStream,
            string streamContentType,
            IAuthenticationHeaderGenerator authenticationHeaderGenerator,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            return await client.PatchWithAuthenticationAsync(
                requestUri,
                contentDataStream,
                streamContentType,
                81920,
                authenticationHeaderGenerator,
                logger,
                cancellationToken).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> PatchWithAuthenticationAsync(
            this HttpClient client,
            string requestUri,
            Stream contentDataStream,
            string streamContentType,
            int bufferSize,
            IAuthenticationHeaderGenerator authenticationHeaderGenerator,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            client.DefaultRequestHeaders.Authorization =
                authenticationHeaderGenerator.GenerateAuthenticationHeader(requestUri);

            using (var content = new StreamContent(contentDataStream, bufferSize)
            {
                Headers =
                    {
                        ContentType = new MediaTypeHeaderValue(streamContentType),
                    }
            })
            {
                logger?.LogDebug("Added auth header to {Verb} call to {RequestUri}", "PATCH", requestUri);
                return await client.SendAsync(
                    new HttpRequestMessage(new HttpMethod("PATCH"), requestUri)
                    {
                        Content = content,
                        Headers =
                        {
                            TransferEncodingChunked = true,
                        }
                    },
                    cancellationToken).ConfigureAwait(false);
            };
        }
    }
}
