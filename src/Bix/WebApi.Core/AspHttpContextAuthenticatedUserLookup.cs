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

using Bix.Http.Core;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics.Contracts;

namespace Bix.WebApi.Core
{
    /// <summary>
    /// Looks up the current authenticated user within an ASPNETCore web service.
    /// Pairing with an optional <see cref="IAspHttpContextAuthenticatedUserLookupConfig"/>
    /// allows for the setting of a default user to be sent in case no user is authenticated, such
    /// as in the case of automated jobs/tasks.
    /// </summary>
    public class AspHttpContextAuthenticatedUserLookup : IAuthenticatedUserLookup
    {
        /// <summary>
        /// Creates a new <see cref="AspHttpContextAuthenticatedUserLookup"/>
        /// </summary>
        /// <param name="httpContextAccessor">Http context accessor to use for the user lookup</param>
        /// <param name="config">Optional configuration that, if given, provides a default name to be used by system actions.</param>
        /// <remarks>
        /// A default application username might be used, for example, by actions that run at startup or based on a timer or
        /// in any scenario outside of a typical incoming request
        /// </remarks>
        public AspHttpContextAuthenticatedUserLookup(
            IHttpContextAccessor httpContextAccessor,
            IAspHttpContextAuthenticatedUserLookupConfig config = null)
        {
            Contract.Requires(httpContextAccessor != null);

            this.Config = config;
            this.HttpContextAccessor = httpContextAccessor;
        }

        private IHttpContextAccessor HttpContextAccessor { get; }
        private IAspHttpContextAuthenticatedUserLookupConfig Config { get; }

        /// <summary>
        /// Retrieves the currently authenticated user
        /// </summary>
        /// <returns>Currently authenticated user</returns>
        public string GetAuthenticatedUser()
        {
            var name =  this.HttpContextAccessor?.HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrEmpty(name) && this.Config != null)
            {
                name = name ?? this.Config.ApplicationUserName;
            }
            return name ?? string.Empty;
        }
    }
}
