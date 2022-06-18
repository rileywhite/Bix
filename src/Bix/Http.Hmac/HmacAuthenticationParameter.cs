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

using System;

namespace Bix.Http.Hmac
{
    public class HmacAuthenticationParameter
    {
        public string ApplicationKey { get; set; }

        // null/"" coercion happens on json round-tripping
        // since this is an optional field, handle it nicely
        private string authenticatedUser;
        public string AuthenticatedUser
        {
            get => this.authenticatedUser ?? string.Empty;
            set => this.authenticatedUser = value;
        }

        public string Time { get; set; }
        public string Hash { get; set; }

        public HmacAuthenticationParameter CloneWithoutHash()
        {
            return new HmacAuthenticationParameter
            {
                ApplicationKey = this.ApplicationKey,
                AuthenticatedUser = this.AuthenticatedUser,
                Time = this.Time,
            };
        }
    }
}
