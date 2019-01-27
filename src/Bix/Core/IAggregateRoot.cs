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

namespace Bix.Core
{
    /// <summary>
    /// Marks a type as the root of an aggregate, or as the item that can be saved to
    /// or retreived from a <see cref="IRepository{TIdentity, TItem}"/>.
    /// </summary>
    /// <remarks>
    /// Aggregates are alway retrieved/saved as a unit by way of the repository of the root of
    /// the aggregate.
    /// 
    /// See https://martinfowler.com/bliki/DDD_Aggregate.html.
    /// 
    /// This interface does not add any explicit functional contract.
    /// </remarks>
    public interface IAggregateRoot { }
}
