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
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Bix.Mixers.CecilMixer
{
    internal class EncapsulateMixer
    {
        public void AddEncapsulation(string modulePath, ModuleDefinition typeModule)
        {
            var originalTypes = new List<TypeDefinition>(typeModule.Types);
            var encapsulatesAttributeType = typeModule.Import(typeof(EncapsulatesAttribute)).Resolve();
            foreach (var type in originalTypes)
            {
                var encapsulatesAttribute = type.CustomAttributes.SingleOrDefault(
                    customAttribute => customAttribute.AttributeType.Resolve() == encapsulatesAttributeType);

                if (encapsulatesAttribute != null)
                {
                    AddEncapsulation(typeModule, type);
                    type.CustomAttributes.Remove(encapsulatesAttribute);
                }
            }

            typeModule.Write(modulePath, new WriterParameters { SymbolWriterProvider = new PdbWriterProvider() });
        }

        private void AddEncapsulation(ModuleDefinition typeModule, TypeDefinition type)
        {
            this.AddIEncapsulates(typeModule, type);
        }

        private void AddIEncapsulates(ModuleDefinition typeModule, TypeDefinition type)
        {
            type.Interfaces.Add(typeModule.Import(typeof(IEncapsulates)));

            AddDataTransferObject(typeModule, type);

            var mixersProperty = type.ImplementAutoPropertyExplicitly(typeof(IEncapsulates).GetProperty("Encapsulator"));
            mixersProperty.MarkAsCompilerGenerated();
        }

        private static void AddDataTransferObject(ModuleDefinition typeModule, TypeDefinition type)
        {
            var dtoType = new TypeDefinition(string.Empty, "Dto", TypeAttributes.NestedPublic | TypeAttributes.Class, typeModule.Import(typeof(object)));
            type.NestedTypes.Add(dtoType);
            dtoType.AddPublicConstructor(
                ilProcessor =>
                {
                    ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
                    ilProcessor.Append(Instruction.Create(OpCodes.Call, typeModule.ImportConstructor(typeof(object))));
                    ilProcessor.Append(Instruction.Create(OpCodes.Ret));
                });

            var encapsulatedAttributeType = typeModule.Import(typeof(EncapsulatedAttribute)).Resolve();
            foreach (var property in type.Properties)
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
