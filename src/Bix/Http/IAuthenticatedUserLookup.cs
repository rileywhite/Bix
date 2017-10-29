using System;

namespace Bix.Http
{
    public interface IAuthenticatedUserLookup
    {
        string GetAuthenticatedUser();
    }
}
