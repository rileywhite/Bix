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

namespace Bix.Autofac
{
    public enum AutofacModuleInstanceScope
    {
        /// <summary>
        /// Potentially a good fit for desktop apps
        /// </summary>
        SingleInstance,

        /// <summary>
        /// Potentially a good fit for mvc/web api apps
        /// </summary>
        InstancePerLifetimeScope,

        /// <summary>
        /// Potentially a good fit for web forms apps
        /// </summary>
        InstancePerRequest,

        /// <summary>
        /// Provided by Hangfire.Autofac for use in Hangfire queued background jobs.
        /// Used for a single instance per background task.
        /// </summary>
        InstancePerBackgroundJob,
    }
}
