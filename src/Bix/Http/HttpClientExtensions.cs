using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Bix.Http
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PatchAsync(
            this HttpClient client,
            string requestUri,
            HttpContent content,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await client.SendAsync(
                new HttpRequestMessage(new HttpMethod("PATCH"), requestUri) { Content = content }, cancellationToken);
        }
    }
}
