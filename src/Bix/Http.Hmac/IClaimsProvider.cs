using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Bix.Http.Hmac
{
    public interface IClaimsProvider
    {
        IEnumerable<Claim> AddClaimsTo(ClaimsIdentity identity, AuthenticationProperties authenticationProperties);
    }
}
