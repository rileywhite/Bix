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

namespace Bix.WebApi.Core
{
    /// <summary>
    /// When implemented by a concrete type, can be passed into the constructor of
    /// a <see cref="AspHttpContextContextAuthenticatedUserLookup"/> to provide
    /// a username to be used by the Application to perform actions that are not associated
    /// with an active incoming request, such as startup, shutdown, etc.
    /// </summary>
    public interface IAspHttpContextContextAuthenticatedUserLookupConfig
    {
        /// <summary>
        /// Name of the User that represents the currently running application
        /// </summary>
        string ApplicationUserName { get; }
    }
}
