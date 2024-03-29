﻿/***************************************************************************/
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
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using NodaTime.Text;
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Text.Encodings.Web;

namespace Bix.Http.Hmac
{
    /// <summary>
    /// Authentication that delegates identity management to calling applications and, once
    /// the calling app is verified using HMAC, trusts the identity assignment
    /// </summary>
    public class HmacAuthenticationHandler : AuthenticationHandler<HmacAuthenticationSchemeOptions>
    {
        public const string HmacSchemeName = "hmac";
        public const string TrustedNetworkSchemeName = "trusted_network";

        private new ILogger<HmacAuthenticationHandler> Logger { get; }

        public HmacAuthenticationHandler(
            IOptionsMonitor<HmacAuthenticationSchemeOptions> options,
            ILoggerFactory loggerFactory,
            UrlEncoder encoder,
            ISystemClock clock,
            IApplicationSecretStore applicationSecretStore,
            IClaimsProvider claimsProvider)
            : base(options, loggerFactory, encoder, clock)
        {
            Contract.Requires(applicationSecretStore != null);
            Contract.Ensures(this.ApplicationSecretStore != null);

            this.Logger = loggerFactory?.CreateLogger<HmacAuthenticationHandler>();

            this.ApplicationSecretStore = applicationSecretStore;
            this.ClaimsProvider = claimsProvider;
        }

        private IApplicationSecretStore ApplicationSecretStore { get; }

        private IClaimsProvider ClaimsProvider { get; }

        protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authorizationHeader = this.Request.Headers["Authorization"];

            var remoteIPAddress = this.Context.Connection.RemoteIpAddress;
            this.Logger?.LogDebug(
                "Request came from remote IP Address: {RemoteIPAddress}. Continuing with auth check.",
                remoteIPAddress.ToJson());

            if (!authorizationHeader.Any())
            {
                this.Logger?.LogWarning(
                    "Failing authentication from IP {RemoteIPAddress} for application: {Reason}",
                    remoteIPAddress.ToJson(),
                    "No authorization header found");
                return AuthenticateResult.Fail("No authorization header found");
            }

            if (!TryGetHmacAuthenticationParameter(this.Logger, remoteIPAddress, authorizationHeader[0] ?? string.Empty, out var parameter))
            {
                this.Logger?.LogWarning("Failing authentication from IP {RemoteIPAddress}: {Reason}",
                    remoteIPAddress.ToJson(),
                    "Invalid HMAC authentication parameter");
                return AuthenticateResult.Fail("Invalid HMAC authentication parameter");
            }

            if (!this.ApplicationSecretStore.TryGetValue(parameter.ApplicationKey, out var applicationSecret))
            {
                this.Logger?.LogWarning("Failing authentication from IP {RemoteIPAddress}: {Reason}. Parameters: {Parameters}",
                    remoteIPAddress.ToJson(),
                    "No secret found for application key",
                    parameter.ToJson());
                return AuthenticateResult.Fail("No secret found for application key");
            }

            if (!IsRequestFresh(parameter, out var freshnessIndicator))
            {
                this.Logger?.LogWarning(
                    "Failing authentication from IP {RemoteIPAddress}: {Reason}. Parameters: {Parameters}. Freshness Indicator: {FreshnessIndicator}",
                    remoteIPAddress.ToJson(),
                    "HMAC authentication request is not fresh",
                    parameter.ToJson(),
                    freshnessIndicator.ToJson());
                return AuthenticateResult.Fail("HMAC authentication request is not fresh");
            }

            var includeBodyInHash = !parameter.ForceExcludeBody && this.Request.ContentLength.HasValue && this.Request.ContentLength.Value > 0;
            var requestBody = includeBodyInHash ? await ReadRequestBody(this.Request).ConfigureAwait(false) : string.Empty;
            var displayUrl = this.Request.GetDisplayUrl();

            var hash = HmacHashGenerator.Generate(
                parameter,
                applicationSecret,
                displayUrl,
                requestBody);

            if (hash != parameter.Hash)
            {
                this.Logger?.LogWarning(
                    "Failing authentication from IP {RemoteIPAddress}: {Reason}. DisplayUrl: {DisplayUrl}. Parameters: {Parameters}. Includes body: {IncludeBodyInHash}.",
                    remoteIPAddress.ToJson(),
                    "HMAC request hashes do not match",
                    displayUrl,
                    parameter.ToJson(),
                    includeBodyInHash);
                return AuthenticateResult.Fail("HMAC request hashes do not match");
            }

            this.Logger?.LogDebug(
                "HMAC authentication from IP {RemoteIPAddress} looks good. Building claims principal for User: {User}, ApplicationKey: {Application}, Time: {Time}.",
                remoteIPAddress.ToJson(),
                parameter.AuthenticatedUser,
                parameter.ApplicationKey,
                parameter.Time);

            var properties = new AuthenticationProperties();
            properties.Items[nameof(HmacAuthenticationParameter.ApplicationKey)] = parameter.ApplicationKey;
            properties.Items[nameof(HmacAuthenticationParameter.AuthenticatedUser)] = parameter.AuthenticatedUser;
            properties.Items[nameof(HmacAuthenticationParameter.Time)] = parameter.Time;

            ClaimsIdentity identity = new GenericIdentity(parameter.AuthenticatedUser, HmacSchemeName);

            if (this.ClaimsProvider == null)
            {
                this.Logger?.LogWarning("The claims provider is null. Proceeding without attaching claims to identity.");
            }
            else
            {
                this.Logger?.LogDebug("Attaching claims to identity.");
                this.ClaimsProvider.AddClaimsTo(identity, properties);

                if (identity.Claims.Any()) { this.Logger?.LogDebug("Claims were successfully attached."); }
                else { this.Logger?.LogWarning("No claims were attached by the claims provider."); }
            }

            var user = new ClaimsPrincipal(identity);

            this.Logger?.LogDebug("Returning success with claims principal.");

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

        private static bool TryGetHmacAuthenticationParameter(
            ILogger logger,
            IPAddress remoteIPAddress,
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
                logger?.LogWarning(
                    ex,
                    "Bad HMAC authentication header from IP {RemoteIPAddress}: {AuthorizationHeaderValue}",
                    remoteIPAddress.ToJson(),
                    authorizationHeaderValue);

                parameter = null;
                return false;
            }
        }

        private static bool IsRequestFresh(HmacAuthenticationParameter parameter, out Tuple<ParseResult<Instant>, Instant, Duration> freshnessIndicators)
        {
            var now = NodaTime.SystemClock.Instance.GetCurrentInstant();

            var parseResult = InstantPattern.General.Parse(parameter.Time);
            if (!parseResult.Success)
            {
                freshnessIndicators = Tuple.Create(parseResult, now, Duration.MaxValue);
                return false; 
            }

            var offset = now - parseResult.Value;
            if (offset < Duration.Zero) { offset = Duration.Negate(offset); }

            var isRequestFresh = offset < Duration.FromMinutes(15);

            freshnessIndicators = Tuple.Create(parseResult, now, offset);

            return isRequestFresh;
        }
    }
}
