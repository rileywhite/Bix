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

using Newtonsoft.Json;
using System;

namespace Bix.Core
{
    /// <summary>
    /// Base for types that can be stored in a <see cref="IRepository{TIdentity, TItem}"/>.
    /// </summary>
    /// <typeparam name="TIdentity">Type of identifier for this model</typeparam>
    public abstract class ModelBase<TIdentity> : IModel<TIdentity>
    {
        /// <summary>
        /// Gets whether this is an aggregate root, meaning that it is the root
        /// of a graph of models through which the graph can be created, saved, updated,
        /// or deleted.
        /// </summary>
        [JsonIgnore]
        bool IModel.IsAggregateRoot => this.GetType().IsAggregateRootModelType();

        /// <summary>
        /// Identity that uniquely identifies a model stored within a repository.
        /// </summary>
        public abstract TIdentity Identity { get; }
    }
}
