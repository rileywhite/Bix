using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bix.Core
{
    public interface IRepository<TIdentity, TItem>
        where TItem : class, IAggregateRoot, IHasIdentity<TIdentity>
    {
        Task<IQueryable<TItem>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<TItem> FindAsync(TIdentity identity, CancellationToken cancellationToken = default(CancellationToken));
        Task<TItem> AddAsync(TItem item, CancellationToken cancellationToken = default(CancellationToken));
        Task RemoveAsync(TIdentity identity, CancellationToken cancellationToken = default(CancellationToken));
        Task<TItem> UpdateAsync(TItem item, CancellationToken cancellationToken = default(CancellationToken));
        Task<ModelMetadata> GetMetadataAsync(TIdentity identity, CancellationToken cancellationToken = default(CancellationToken));
    }
}
