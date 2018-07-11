using System;
using System.Collections.Generic;
using System.Text;

namespace Bix.Http.Hmac
{
    public interface IApplicationSecretStore : IReadOnlyDictionary<string, Guid>
    {
    }
}
