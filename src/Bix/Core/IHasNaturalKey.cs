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
    /// Allows a type to declare a natural key
    /// </summary>
    /// <typeparam name="TNaturalKey">Type of the natural key</typeparam>
    /// <remarks>
    /// A typical use case for this is DDD value objects (https://martinfowler.com/bliki/ValueObject.html)
    /// </remarks>
    public interface IHasNaturalKey<TNaturalKey>
    {
        /// <summary>
        /// Gets the natural key of an instance
        /// </summary>
        TNaturalKey NaturalKey { get; }
    }
}
