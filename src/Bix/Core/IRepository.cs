/***************************************************************************/
// Copyright 2013-2019 Riley White
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
    /// <summary>
    /// Defines an interface that, when implemented, can be used to perform CRUD operations on <see cref="IAggregateRoot"/> models.
    /// </summary>
    /// <typeparam name="TIdentity">Type of unique identifiers used by the <see cref="IAggregateRoot"/> models.</typeparam>
    /// <typeparam name="TItem">Type of the <see cref="IAggregateRoot"/> that can be operated on by the repository.</typeparam>
    public interface IRepository<TIdentity, TItem>
        where TItem : class, IAggregateRoot, IModel<TIdentity>
    {
        /// <summary>
        /// Retrieves all items in the repository.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for cancelling the operation.</param>
        /// <returns>Queryable used to access all items in the repository</returns>
        Task<IQueryable<TItem>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a single item by its ID
        /// </summary>
        /// <param name="identity">Value that identifies a unique item</param>
        /// <param name="cancellationToken">Cancellation token for cancelling the operation.</param>
        /// <returns>Found item or <c>null</c> if not found</returns>
        Task<TItem> FindAsync(TIdentity identity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds an item to the repoisitory
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <param name="cancellationToken">Cancellation token for cancelling the operation.</param>
        /// <returns>Added item, including any repo-generated values, such as IDs</returns>
        Task<TItem> AddAsync(TItem item, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an item from the repository
        /// </summary>
        /// <param name="identity">ID of item to remove</param>
        /// <param name="cancellationToken">Cancellation token for cancelling the operation.</param>
        /// <returns>Async task</returns>
        Task RemoveAsync(TIdentity identity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an item that exists in the repository
        /// </summary>
        /// <param name="item">Item with updated information. ID of the item will be used to find and update version in the repository.</param>
        /// <param name="cancellationToken">Cancellation token for cancelling the operation.</param>
        /// <returns>Updated item, including any server-generated values</returns>
        Task<TItem> UpdateAsync(TItem item, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets <see cref="ModelMetadata"/> for an item in the repository.
        /// </summary>
        /// <param name="identity">ID of the item to get metadata for</param>
        /// <param name="cancellationToken">Cancellation token for cancelling the operation.</param>
        /// <returns>Metadata for the item or <c>null</c> if the item is not found</returns>
        Task<ModelMetadata> GetMetadataAsync(TIdentity identity, CancellationToken cancellationToken = default);
    }
}
