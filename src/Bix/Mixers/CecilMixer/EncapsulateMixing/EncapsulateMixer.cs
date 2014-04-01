/***************************************************************************/
// EncapsulateMixer.cs
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

using Bix.Mix;
using Bix.Mix.Encapsulate;
using Bix.Mixers.CecilMixer.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;

namespace Bix.Mixers.CecilMixer.EncapsulateMixing
{
    internal class EncapsulateMixer
    {
        public void Mix(ModuleDefinition targetModule)
        {
            Contract.Requires(targetModule != null);

            var originalTypes = new List<TypeDefinition>(targetModule.Types);
            var encapsulatesAttributeType = targetModule.Import(typeof(EncapsulatesAttribute)).Resolve();
            foreach (var type in originalTypes)
            {
                var encapsulatesAttribute = type.CustomAttributes.SingleOrDefault(
                    customAttribute => customAttribute.AttributeType.Resolve() == encapsulatesAttributeType);

                if (encapsulatesAttribute != null)
                {
                    AddEncapsulation(type);
                    type.CustomAttributes.Remove(encapsulatesAttribute);
                }
            }
        }

        private void AddEncapsulation(TypeDefinition target)
        {
            this.AddIEncapsulates(target);
        }

        private void AddIEncapsulates(TypeDefinition target)
        {
            Contract.Requires(target != null);

            new InterfaceMixCommand<IEncapsulates, EncapsulateSource>(target).Mix();
            //target.Interfaces.Add(target.Module.Import(typeof(IEncapsulates)));
            //var mixersProperty = type.ImplementAutoPropertyExplicitly(typeof(IEncapsulates).GetProperty("Encapsulator"));
            //mixersProperty.MarkAsCompilerGenerated();
            AddDataTransferObject(target);
        }

        private static void AddDataTransferObject(TypeDefinition target)
        {
            Contract.Requires(target != null);

            var typeModule = target.Module;
            var dtoType = new TypeDefinition(string.Empty, "Dto", TypeAttributes.NestedPublic | TypeAttributes.Class, typeModule.Import(typeof(object)));
            target.NestedTypes.Add(dtoType);
            dtoType.AddPublicConstructor(
                ilProcessor =>
                {
                    ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
                    ilProcessor.Append(Instruction.Create(OpCodes.Call, typeModule.ImportConstructor(typeof(object))));
                    ilProcessor.Append(Instruction.Create(OpCodes.Ret));
                });

            var encapsulatedAttributeType = typeModule.Import(typeof(EncapsulatedAttribute)).Resolve();
            foreach (var property in target.Properties)
            {
                var encapsulatedAttribute = property.CustomAttributes.SingleOrDefault(
                    customAttribute => customAttribute.AttributeType.Resolve() == encapsulatedAttributeType);

                if (encapsulatedAttribute != null)
                {
                    dtoType.AddAutoProperty(property.Name, property.PropertyType);
                    property.CustomAttributes.Remove(encapsulatedAttribute);
                }
            }
        }
    }
}
