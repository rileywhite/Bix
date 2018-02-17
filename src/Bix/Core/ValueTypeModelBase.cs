/***************************************************************************/
// Copyright 2013-2018 Riley White
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
    public abstract class ValueTypeModelBase<TModel, TIdentity, TNaturalKey>
        : ModelBase<TModel, TIdentity>, IHasNaturalKey<TNaturalKey>
        where TModel : ValueTypeModelBase<TModel, TIdentity, TNaturalKey>, new()
    {
        public abstract TNaturalKey NaturalKey { get; }
    }

    public abstract class ValueTypeModelBase<TModel, TNaturalKey>
        : ValueTypeModelBase<TModel, TNaturalKey, TNaturalKey>
        where TModel : ValueTypeModelBase<TModel, TNaturalKey>, new()
    {
        public override sealed TNaturalKey Identity => this.NaturalKey;
    }
}
