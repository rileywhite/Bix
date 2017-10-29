using System;
using System.Net.Http.Headers;

namespace Bix.Http
{
    public interface IAuthenticationHeaderGenerator
    {
        AuthenticationHeaderValue GenerateAuthenticationHeader(
            string endpointUri,
            string jsonContent = null);
    }
}
