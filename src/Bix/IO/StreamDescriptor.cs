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

namespace Bix.IO
{
    /// <summary>
    /// Represents info about a full stream of data that is to be transferred from a source to a target.
    /// </summary>
    public class StreamDescriptor
    {
        /// <summary>
        /// Gets or sets an identifier that uniquely identifies a stream of data
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Length of the data to be transferred.
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// Name of the hash algorithm used for associated segment hashses, e.g. "MD5"
        /// </summary>
        public string HashName { get; set; }
    }
}
