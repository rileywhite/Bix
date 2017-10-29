using Bix.Repositories.Restful.WebApi.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Bix.Repositories.Restful.WebApi
{
    [Authorize(AuthenticationSchemes = HmacAuthenticationHandler.HmacSchemeName)]
    public abstract class BixControllerBase : Controller { }
}
