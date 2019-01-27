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

using Autofac;
using Autofac.Builder;
using Hangfire;
using System;

namespace Bix.Autofac
{
    public static class AutofacExtensions
    {
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle>
            AppendScope<TLimit, TActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> source,
                AutofacModuleInstanceScope scope)
        {
            switch (scope)
            {
                case AutofacModuleInstanceScope.InstancePerLifetimeScope:
                    return source.InstancePerLifetimeScope();

                case AutofacModuleInstanceScope.InstancePerRequest:
                    return source.InstancePerRequest();

                case AutofacModuleInstanceScope.InstancePerBackgroundJob:
                    return source.InstancePerBackgroundJob();

                default:
                    return source.SingleInstance();
            }
        }
    }
}
