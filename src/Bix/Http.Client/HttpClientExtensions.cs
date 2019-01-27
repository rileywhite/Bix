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

using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bix.Http.Client
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PatchAsync(
            this HttpClient client,
            string requestUri,
            HttpContent content,
            CancellationToken cancellationToken = default)
        {
            return await client.SendAsync(
                new HttpRequestMessage(new HttpMethod("PATCH"), requestUri) { Content = content }, cancellationToken);
        }

        public static async Task<HttpResponseMessage> PatchAsync(
            this HttpClient client,
            string requestUri,
            string jsonContent,
            CancellationToken cancellationToken = default)
        {
            return await client.PatchAsync(requestUri, new StringContent(jsonContent, Encoding.UTF8, "application/json"), cancellationToken);
        }
    }
}
