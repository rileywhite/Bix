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

using af = Autofac;
using Autofac;
using System;
using System.Reflection;

namespace Bix.Autofac
{
    public abstract class RepositoryAutofacModuleBase : af.Module
    {
        public RepositoryAutofacModuleBase(
            Func<Type, bool> typePredicate,
            AutofacModuleInstanceScope scope = AutofacModuleInstanceScope.SingleInstance)
        {
            this.TypePredicate = typePredicate;
            this.Scope = scope;
        }

        public Func<Type, bool> TypePredicate { get; }
        public AutofacModuleInstanceScope Scope { get; }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterAssemblyTypes(this.GetType().GetTypeInfo().Assembly)
                .Where(this.TypePredicate)
                .AsImplementedInterfaces()
                .AppendScope(this.Scope);
        }
    }
}
