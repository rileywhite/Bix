using System;

namespace Bix.Http.Hmac
{
    public interface IApplicationSecrets
    {
        string ApplicationKey { get; }
        Guid ApplicationSecretKey { get; }
    }
}
