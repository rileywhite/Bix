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
