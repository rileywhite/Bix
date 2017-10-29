using Bix.Core;
using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Bix.Http.Hmac
{
    public static class HmacHashGenerator
    {
        public static string Generate(
            HmacAuthenticationParameter parameter,
            Guid secretKey,
            string endpointUri,
            string jsonContent = null)
        {
            var parameterContainer = new
            {
                EndpointUri = WebUtility.UrlDecode(endpointUri),
                JsonContent = jsonContent ?? string.Empty,
                Parameter = parameter.CloneWithoutHash(),
            };

            using (var generator = new HMACSHA512 { Key = secretKey.ToByteArray() })
            {
                return Convert.ToBase64String(
                    generator.ComputeHash(
                        Encoding.UTF8.GetBytes(parameterContainer.ToJson())));
            }
        }
    }
}
