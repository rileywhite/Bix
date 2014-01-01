/***************************************************************************/
// "InitializeResult".cs
// Copyright 2013 Riley White
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
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Bix.Mix
{
    public class InitializeResult
    {
        public InitializeResult(Stopwatch stopwatch)
        {
            Contract.Requires(stopwatch != null);
            Contract.Requires(stopwatch != null);
            Contract.Requires(!stopwatch.IsRunning);
            Contract.Requires(stopwatch.Elapsed >= TimeSpan.Zero);

            Contract.Ensures(this.InitializeTime >= TimeSpan.Zero);

            this.InitializeTime = stopwatch.Elapsed;
        }

        public TimeSpan InitializeTime { get; set; }
    }
}
