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
            if (endpointUri == null)
            {
                throw new ArgumentNullException(nameof(endpointUri));
            }

            if (endpointUri == string.Empty)
            {
                throw new ArgumentException("Cannot be empty", nameof(endpointUri));
            }

            // normalize any URL encoded characters
            endpointUri = WebUtility.UrlDecode(endpointUri);

            // exclude the endpointUri scheme if it exists because reverse proxies will sometimes changing the scheme
            var slashSlashIndex = endpointUri.IndexOf("//");
            if (slashSlashIndex >= 0)
            {
                endpointUri = endpointUri.Substring(slashSlashIndex + 1);
            }

            var parameterContainer = new
            {
                EndpointUri = endpointUri,
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
