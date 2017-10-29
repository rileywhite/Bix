using System;

namespace Bix.Core
{
    public interface IHasIdentity<TIdentity>
    {
        TIdentity Identity { get; }
    }
}
