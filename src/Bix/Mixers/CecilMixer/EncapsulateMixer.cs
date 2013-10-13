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

using Bix.Mix.Encapsulate;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Bix.Mixers.CecilMixer
{
    internal class EncapsulateMixer
    {
        public void AddEncapsulation(string assemblyPath, string classFullName)
        {
            var typeModule = ModuleDefinition.ReadModule(assemblyPath);
            var type = typeModule.GetType(classFullName);

            // add interface to type
            type.Interfaces.Add(typeModule.Import(typeof(IEncapsulates)));

            // create inner DTO
            var dtoType = new TypeDefinition(string.Format("{0}/{1}", type.Namespace, type.Name), "Dto", TypeAttributes.NestedPublic | TypeAttributes.Class, typeModule.Import(typeof(object)));
            var encapsulatedAttributeType = typeModule.Import(typeof(EncapsulatedAttribute)).Resolve();
            foreach (var property in type.Properties)
            {
                if (property.CustomAttributes.Any(attribute => attribute.AttributeType.Resolve() == encapsulatedAttributeType))
                {
                    var dtoField = new FieldDefinition(
                        string.Format("fieldFor{0}_{1}", property.Name, Guid.NewGuid().ToString("N")),
                        FieldAttributes.Private | FieldAttributes.SpecialName,
                        property.PropertyType);
                    dtoField.DeclaringType = dtoType;
                    dtoType.Fields.Add(dtoField);
                    var dtoProperty = new PropertyDefinition(property.Name, PropertyAttributes.None, property.PropertyType);
                    dtoProperty.DeclaringType = dtoType;

                    var dtoPropertyGetter = new MethodDefinition(
                        string.Format("get_{0}", property.Name),
                        MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                        property.PropertyType);
                    dtoPropertyGetter.DeclaringType = dtoType;
                    var ilProcessor = dtoPropertyGetter.Body.GetILProcessor();
                    ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0)); // this
                    ilProcessor.Append(Instruction.Create(OpCodes.Ldfld, dtoField));
                    ilProcessor.Append(Instruction.Create(OpCodes.Ret));
                    dtoProperty.GetMethod = dtoPropertyGetter;
                    dtoType.Methods.Add(dtoPropertyGetter);
                    
                    var dtoPropertySetter = new MethodDefinition(
                        string.Format("set_{0}", property.Name),
                        MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                        typeModule.Import(typeof(void)));
                    dtoPropertySetter.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.In, property.PropertyType));
                    dtoPropertySetter.DeclaringType = dtoType;
                    ilProcessor = dtoPropertySetter.Body.GetILProcessor();
                    ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
                    ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_1));
                    ilProcessor.Append(Instruction.Create(OpCodes.Stfld, dtoField));
                    ilProcessor.Append(Instruction.Create(OpCodes.Ret));
                    dtoProperty.SetMethod = dtoPropertySetter;
                    dtoType.Methods.Add(dtoPropertySetter);

                    dtoType.Properties.Add(dtoProperty);
                }
            }
            type.NestedTypes.Add(dtoType);

            typeModule.Write(assemblyPath, new WriterParameters { SymbolWriterProvider = new PdbWriterProvider() });
        }
    }
}
