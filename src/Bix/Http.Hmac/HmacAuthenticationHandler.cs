﻿/***************************************************************************/
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

using Bix.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using NodaTime.Text;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Text.Encodings.Web;
using System.Diagnostics.Contracts;

namespace Bix.Http.Hmac
{
    /// <summary>
    /// Authentication that delegates identity management to calling applications and, once
    /// the calling app is verified using HMAC, trusts the identity assignment
    /// </summary>
    public class HmacAuthenticationHandler : AuthenticationHandler<HmacAuthenticationSchemeOptions>
    {
        public const string HmacSchemeName = "hmac";

        public HmacAuthenticationHandler(
            IOptionsMonitor<HmacAuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IApplicationSecretStore applicationSecretStore)
            : base(options, logger, encoder, clock)
        {
            Contract.Requires(applicationSecretStore != null);
            Contract.Ensures(this.ApplicationSecretStore != null);

            this.ApplicationSecretStore = applicationSecretStore;
        }

        private IApplicationSecretStore ApplicationSecretStore { get; }

        protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authorizationHeader = this.Request.Headers["Authorization"];

            if (!authorizationHeader.Any())
            {
                return AuthenticateResult.Fail("No authorization header found");
            }

            if (!this.TryGetHmacAuthenticationParameter(authorizationHeader[0] ?? string.Empty, out var parameter))
            {
                return AuthenticateResult.Fail("Invalid HMAC authentication parameter");
            }

            if (!this.ApplicationSecretStore.TryGetValue(parameter.ApplicationKey, out var applicationSecret))
            {
                return AuthenticateResult.Fail("No secret found for application key");
            }

            if (!IsRequestFresh(parameter))
            {
                return AuthenticateResult.Fail("HMAC authentication request is not fresh");
            }

            var includeBodyInHash = this.Request.ContentLength.HasValue && this.Request.ContentLength.Value > 0;
            var requestBody = includeBodyInHash ? await ReadRequestBody(this.Request).ConfigureAwait(false) : string.Empty;

            var hash = HmacHashGenerator.Generate(
                parameter,
                applicationSecret,
                this.Request.GetDisplayUrl(),
                requestBody);

            if (hash != parameter.Hash)
            {
                return AuthenticateResult.Fail("HMAC request hashes do not match");
            }

            var user = new ClaimsPrincipal(new GenericIdentity(parameter.AuthenticatedUser, HmacSchemeName));

            var properties = new AuthenticationProperties();
            properties.Items[nameof(HmacAuthenticationParameter.ApplicationKey)] = parameter.ApplicationKey;
            properties.Items[nameof(HmacAuthenticationParameter.AuthenticatedUser)] = parameter.AuthenticatedUser;
            properties.Items[nameof(HmacAuthenticationParameter.Time)] = parameter.Time;

            return AuthenticateResult.Success(new AuthenticationTicket(user, properties, HmacSchemeName));
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            return base.HandleChallengeAsync(properties);
        }

        private static async Task<string> ReadRequestBody(HttpRequest request)
        {
            request.EnableBuffering();
            try
            {
                // This is safe since Dispose(false), invoked by the StreamReader finalizer,
                // does not close the underlying stream.
                return await new StreamReader(request.Body).ReadToEndAsync().ConfigureAwait(false);
            }
            finally
            {
                request.Body.Position = 0;
            }
        }

        private bool TryGetHmacAuthenticationParameter(
            string authorizationHeaderValue,
            out HmacAuthenticationParameter parameter)
        {
            var headerValues = authorizationHeaderValue.Split(
                new char[] { ' ' },
                2,
                StringSplitOptions.RemoveEmptyEntries);

            if (headerValues.Length != 2 || headerValues[0] != HmacSchemeName)
            {
                parameter = null;
                return false;
            }

            try
            {
                parameter = headerValues[1].ConvertFromJson<HmacAuthenticationParameter>();;
                return true;
            }
            catch (Exception ex)
            {
                this.Logger.LogWarning(ex, "Bad HMAC authentication header {AuthorizationHeaderValue}", authorizationHeaderValue);

                parameter = null;
                return false;
            }
        }

        private static bool IsRequestFresh(HmacAuthenticationParameter parameter)
        {
            var parseResult = InstantPattern.General.Parse(parameter.Time);
            if (!parseResult.Success) { return false; }

            var offset = NodaTime.SystemClock.Instance.GetCurrentInstant() - parseResult.Value;
            if (offset < Duration.Zero) { offset = Duration.Negate(offset); }

            return offset < Duration.FromMinutes(15);
        }
    }
}
