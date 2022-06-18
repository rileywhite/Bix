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
    /// Represents a segment of data within a larger stream that allows for fast comparison
    /// between the segment in a source and a target data transfer.
    /// </summary>
    public class SegmentHash
    {
        /// <summary>
        /// Gets or sets the position within a larger stream of the first byte in a data segment.
        /// </summary>
        public long Start { get; set; }

        /// <summary>
        /// Gets or sets the length within a larger stream of the data segment.
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// Base64 encoded hash of the data segment.
        /// </summary>
        public string Hash { get; set; }
    }
}
