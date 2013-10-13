/***************************************************************************/
// Program.cs
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

namespace Bix.Mixers.CecilMixer
{
    /// <summary>
    /// For now this is impemented as an exe. Still need to think this through,
    /// but it seems likely to change.
    /// </summary>
    class Program
    {
        public static int Main(string[] args)
        {
            new EncapsulateMixer().AddEncapsulation(args[0], args[1]);
            return 0;
        }
    }
}
