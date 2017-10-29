using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bix.Core
{
    public interface IValueTypeRepository<TIdentity, TNaturalKey, TItem> : IRepository<TIdentity, TItem>
        where TItem : class, IAggregateRoot, IHasIdentity<TIdentity>, IHasNaturalKey<TNaturalKey>
    {
        Task<TItem> FindOrAddAsync(TItem item, CancellationToken cancellationToken = default(CancellationToken));
    }

    public interface IValueTypeRepository<TNaturalKey, TItem> : IValueTypeRepository<TNaturalKey, TNaturalKey, TItem>
        where TItem : class, IAggregateRoot, IHasIdentity<TNaturalKey>, IHasNaturalKey<TNaturalKey> { }
}
