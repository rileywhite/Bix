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
using NodaTime;
using NodaTime.Text;
using System;
using System.Net.Http.Headers;

namespace Bix.Http.Hmac
{
    public class HmacAuthenticationHeaderGenerator : IAuthenticationHeaderGenerator
    {
        public HmacAuthenticationHeaderGenerator(IClock clock, IAuthenticatedUserLookup userLookup, IApplicationSecrets applicationSecrets)
        {
            this.Clock = clock;
            this.UserLookup = userLookup;
            this.ApplicationSecrets = applicationSecrets;
        }

        private IClock Clock { get; }
        private IAuthenticatedUserLookup UserLookup { get; }
        public IApplicationSecrets ApplicationSecrets { get; }

        public AuthenticationHeaderValue GenerateAuthenticationHeader(
            string endpointUri,
            string jsonContent = null)
        {
            var parameter = new HmacAuthenticationParameter
            {
                ApplicationKey = this.ApplicationSecrets.ApplicationKey,
                AuthenticatedUser = this.UserLookup.GetAuthenticatedUser(),
                Time = InstantPattern.General.Format(this.Clock.GetCurrentInstant()),
            };

            parameter.Hash = HmacHashGenerator.Generate(
                parameter,
                this.ApplicationSecrets.ApplicationSecretKey,
                endpointUri,
                jsonContent);

            return new AuthenticationHeaderValue("hmac", parameter.ToJson());
        }
    }
}
