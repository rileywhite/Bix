/***************************************************************************/
// Copyright 2013-2022 Riley White
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
using System.Threading;
using System.Threading.Tasks;

namespace Bix.Core
{
    /// <summary>
    /// When implemented, creates a repository that can store items identified by a natural key as well as an
    /// assigned identity.
    /// </summary>
    /// <typeparam name="TIdentity">Type of the assigned identity.</typeparam>
    /// <typeparam name="TNaturalKey">Type of the natural key</typeparam>
    /// <typeparam name="TItem">Type of the stored item</typeparam>
    public interface IValueTypeRepository<TIdentity, TNaturalKey, TItem> : IRepository<TIdentity, TItem>
        where TItem : class, IAggregateRoot, IValueTypeModel<TIdentity, TNaturalKey>
    {
        /// <summary>
        /// Given an item with a populated natural key, finds the existing version or adds the given value.
        /// </summary>
        /// <param name="item">Item to find or add</param>
        /// <param name="cancellationToken">Cancellation token for cancelling the operation.</param>
        /// <returns>Found or added item, including any pre-existing or server-generated data.</returns>
        Task<TItem> FindOrAddAsync(TItem item, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// When implemented, creates a repository that can store items identified only by a natural key.
    /// </summary>
    /// <typeparam name="TNaturalKey">Type of the natural key.</typeparam>
    /// <typeparam name="TItem">Type of the stored item</typeparam>
    public interface IValueTypeRepository<TNaturalKey, TItem> : IValueTypeRepository<TNaturalKey, TNaturalKey, TItem>
        where TItem : class, IAggregateRoot, IValueTypeModel<TNaturalKey> { }
}
