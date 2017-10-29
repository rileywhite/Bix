using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bix.Http
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
    }
}
