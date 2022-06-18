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
    /// Base type for a model with an assigned key and a natural key.
    /// </summary>
    /// <typeparam name="TIdentity">Type of the model's ID</typeparam>
    /// <typeparam name="TNaturalKey">Type of the model's natrual key</typeparam>
    public abstract class ValueTypeModelBase<TIdentity, TNaturalKey>
        : ModelBase<TIdentity>, IValueTypeModel<TIdentity, TNaturalKey>
    {
        /// <summary>
        /// Gets the model's natrual key
        /// </summary>
        public abstract TNaturalKey NaturalKey { get; }
    }

    /// <summary>
    /// Base type for a model with a natural key.
    /// </summary>
    /// <typeparam name="TNaturalKey">Type of the model's natrual key</typeparam>
    public abstract class ValueTypeModelBase<TNaturalKey>
        : ValueTypeModelBase<TNaturalKey, TNaturalKey>, IValueTypeModel<TNaturalKey>
    {
        /// <summary>
        /// Same as <see cref="IHasNaturalKey{TNaturalKey}.NaturalKey"/>
        /// </summary>
        public override sealed TNaturalKey Identity => this.NaturalKey;
    }
}
