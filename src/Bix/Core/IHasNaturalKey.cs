using System;

namespace Bix.Core
{
    public interface IHasNaturalKey<TNaturalKey>
    {
        TNaturalKey NaturalKey { get; }
    }
}
