/***************************************************************************/
// MixContext.cs
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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mix
{
    /// <summary>
    /// Currently serves as a point for all items to get mix provider info. This will
    /// likely be expanded with/replaced by some sort of IoC.
    /// </summary>
    //public class MixContext
    //{
    //    private static MixContext Instance { get; set; }

    //    public static void SetFor(IMixes mixes)
    //    {
    //        Contract.Requires(mixes != null);
    //        Contract.Ensures(mixes.MixContext != null);
    //        mixes.MixContext = Instance;
    //    }

    //    public static MixContext GetFrom(IMixes mixes)
    //    {
    //        Contract.Requires(mixes != null);
    //        return mixes.MixContext;
    //    }

    //    public TMixProvider Get<TMixProvider>()
    //        where TMixProvider : IMixProvider
    //    {
    //        Contract.Ensures(Contract.Result<TMixProvider>() != null);
    //        throw new NotImplementedException();
    //    }
    //}
}
