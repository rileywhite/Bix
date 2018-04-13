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

using Bix.Http.Core;
using System;
using System.IO;
using System.Net.Http;
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
            CancellationToken cancellationToken = default(CancellationToken))
        {
            client.DefaultRequestHeaders.Authorization =
                authenticationHeaderGenerator.GenerateAuthenticationHeader(requestUri);
            cancellationToken.ThrowIfCancellationRequested();
            return await client.GetStreamAsync(requestUri);
        }

        public static async Task<HttpResponseMessage> PostWithAuthenticationAsync(
            this HttpClient client,
            string requestUri,
            string jsonContent,
            IAuthenticationHeaderGenerator authenticationHeaderGenerator,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            client.DefaultRequestHeaders.Authorization =
                authenticationHeaderGenerator.GenerateAuthenticationHeader(requestUri, jsonContent);
            return await client.PostAsync(requestUri, new StringContent(jsonContent, Encoding.UTF8, "application/json"), cancellationToken);
        }

        public static async Task<HttpResponseMessage> DeleteWithAuthenticationAsync(
            this HttpClient client,
            string requestUri,
            IAuthenticationHeaderGenerator authenticationHeaderGenerator,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            client.DefaultRequestHeaders.Authorization =
                authenticationHeaderGenerator.GenerateAuthenticationHeader(requestUri);
            return await client.DeleteAsync(requestUri, cancellationToken);
        }

        public static async Task<HttpResponseMessage> PutWithAuthenticationAsync(
            this HttpClient client,
            string requestUri,
            string jsonContent,
            IAuthenticationHeaderGenerator authenticationHeaderGenerator,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            client.DefaultRequestHeaders.Authorization =
                authenticationHeaderGenerator.GenerateAuthenticationHeader(requestUri, jsonContent);
            return await client.PutAsync(requestUri, new StringContent(jsonContent, Encoding.UTF8, "application/json"), cancellationToken);
        }

        public static async Task<HttpResponseMessage> PatchWithAuthenticationAsync(
            this HttpClient client,
            string requestUri,
            string jsonContent,
            IAuthenticationHeaderGenerator authenticationHeaderGenerator,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            client.DefaultRequestHeaders.Authorization =
                authenticationHeaderGenerator.GenerateAuthenticationHeader(requestUri, jsonContent);
            return await client.PatchAsync(requestUri, new StringContent(jsonContent, Encoding.UTF8, "application/json"), cancellationToken);
        }

        public static async Task<HttpResponseMessage> PatchWithAuthenticationAsync(
            this HttpClient client,
            string requestUri,
            Stream contentDataStream,
            IAuthenticationHeaderGenerator authenticationHeaderGenerator,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await client.PatchWithAuthenticationAsync(
                requestUri,
                contentDataStream,
                81920,
                authenticationHeaderGenerator,
                cancellationToken);
        }

        public static async Task<HttpResponseMessage> PatchWithAuthenticationAsync(
            this HttpClient client,
            string requestUri,
            Stream contentDataStream,
            int bufferSize,
            IAuthenticationHeaderGenerator authenticationHeaderGenerator,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            client.DefaultRequestHeaders.Authorization =
                authenticationHeaderGenerator.GenerateAuthenticationHeader(requestUri);
            return await client.PatchAsync(requestUri, new StreamContent(contentDataStream, bufferSize), cancellationToken);
        }
    }
}
