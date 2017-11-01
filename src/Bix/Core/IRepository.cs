/***************************************************************************/
// Copyright 2013-2017 Riley White
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
/***************************************************************************/

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
