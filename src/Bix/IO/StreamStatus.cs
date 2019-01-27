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

namespace Bix.IO
{
    /// <summary>
    /// Holds a descriptor for a data stream and segment hashes for comparing segments of a stream
    /// for use in quickly determining a data transfer restart point
    /// </summary>
    public class StreamStatus
    {
        /// <summary>
        /// Gets or sets the descriptor that provides info about a stream of data to be transferred.
        /// </summary>
        public StreamDescriptor Descriptor { get; set; }

        /// <summary>
        /// Gets or sets a collection of segment hash data objects that can be used to compare
        /// pieces of a source and target stream of data.
        /// </summary>
        public SegmentHash[] SegmentHashes { get; set; }
    }
}
 