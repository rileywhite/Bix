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
