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
    /// <summary>
    /// Represents metadata about a model stored in a repository.
    /// </summary>
    public class ModelMetadata
    {
        /// <summary>
        /// Gets or sets the point in time at which a model was created.
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the ID that represents the user by whom a model was created.
        /// </summary>
        public decimal? CreatedById { get; set; }

        /// <summary>
        /// Gets or sets the point in time at which a model was updated.
        /// </summary>
        public DateTime? LastUpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the ID that represents the user by whom a model was updated.
        /// </summary>
        public decimal? LastUpdatedById { get; set; }
    }
}
