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

namespace Bix.Core
{
    /// <summary>
    /// Contract for types that can be stored within an <see cref="IRepository{TIdentity, TItem}"/>
    /// </summary>
    /// <remarks>
    /// Do not implement this interface directly. Instead implement <see cref="IModel{TModel, TIdentity}"/>.
    /// This interface is primarily for ease of access in cases where the actual type doesn't matter. Normally this is used
    /// internally by repositories.
    /// </remarks>
    public interface IModel
    {
        /// <summary>
        /// Gets whether this is an aggregate root, meaning that it is the root
        /// of a graph of models through which the graph can be created, saved, updated,
        /// or deleted.
        /// </summary>
        bool IsAggregateRoot { get; }
    }

    /// <summary>
    /// Contract for types that can be stored in a <see cref="IRepository{TIdentity, TItem}"/>.
    /// </summary>
    /// <typeparam name="TIdentity">Type of identifier for a <typeparamref name="TModel"/></typeparam>
    public interface IModel<TIdentity> : IModel, IHasIdentity<TIdentity> { }
}
