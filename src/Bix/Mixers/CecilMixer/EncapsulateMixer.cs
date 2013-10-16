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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Bix.Mixers.CecilMixer
{
    internal class EncapsulateMixer
    {
        public void AddEncapsulation(string assemblyPath, string classFullName)
        {
            var typeModule = ModuleDefinition.ReadModule(assemblyPath);
            var type = typeModule.GetType(classFullName);
            var voidTypeReference = typeModule.Import(typeof(void));
            var iEncapsulatesTypeReference = typeModule.Import(typeof(IEncapsulates));
            var iMixesTypeReference = typeModule.Import(typeof(IMixes));
            var iSerializableTypeReference = typeModule.Import(typeof(ISerializable));
            var objectTypeReference = typeModule.Import(typeof(object));
            var objectConstructorReference = typeModule.Import(typeof(object).GetConstructor(new Type[0]));
            var compilerGeneratedAttributeConstructorReference = typeModule.Import(typeof(CompilerGeneratedAttribute).GetConstructor(new Type[0]));
            var encapsulatedAttributeType = typeModule.Import(typeof(EncapsulatedAttribute)).Resolve();
            var serializationInfoTypeReference = typeModule.Import(typeof(SerializationInfo));
            var streamingContextTypeReference = typeModule.Import(typeof(StreamingContext));

            // add interface to type
            type.Interfaces.Add(iEncapsulatesTypeReference);
            type.Interfaces.Add(iMixesTypeReference);
            type.Interfaces.Add(iSerializableTypeReference);
            
            // add serialization methods
            var getObjectDataMethod = new MethodDefinition(
                "System.Runtime.Serialization.ISerializable.GetObjectData",
                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                voidTypeReference);
            getObjectDataMethod.Parameters.Add(new ParameterDefinition("info", ParameterAttributes.None, serializationInfoTypeReference));
            getObjectDataMethod.Parameters.Add(new ParameterDefinition("context", ParameterAttributes.None, streamingContextTypeReference));
            getObjectDataMethod.Overrides.Add(typeModule.Import(typeof(ISerializable).GetMethod("GetObjectData", new Type[] { typeof(SerializationInfo), typeof(StreamingContext) })));
            var ilProcessor = getObjectDataMethod.Body.GetILProcessor();
            ilProcessor.Append(Instruction.Create(OpCodes.Ret));
            getObjectDataMethod.DeclaringType = type;
            type.Methods.Add(getObjectDataMethod);

            var serializationConstructor = new MethodDefinition(
                ".ctor",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                voidTypeReference);
            serializationConstructor.Parameters.Add(new ParameterDefinition("info", ParameterAttributes.None, serializationInfoTypeReference));
            serializationConstructor.Parameters.Add(new ParameterDefinition("context", ParameterAttributes.None, streamingContextTypeReference));
            ilProcessor = serializationConstructor.Body.GetILProcessor();
            ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
            ilProcessor.Append(Instruction.Create(OpCodes.Call, objectConstructorReference));
            ilProcessor.Append(Instruction.Create(OpCodes.Ret));
            serializationConstructor.DeclaringType = type;
            type.Methods.Add(serializationConstructor);


            // create inner DTO
            var dtoType = new TypeDefinition(string.Empty, "Dto", TypeAttributes.NestedPublic | TypeAttributes.Class, objectTypeReference);
            var dtoConstructor = new MethodDefinition(
                ".ctor",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                voidTypeReference);
            ilProcessor = dtoConstructor.Body.GetILProcessor();
            ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
            ilProcessor.Append(Instruction.Create(OpCodes.Call, objectConstructorReference));
            ilProcessor.Append(Instruction.Create(OpCodes.Ret));
            dtoConstructor.DeclaringType = dtoType;
            dtoType.Methods.Add(dtoConstructor);
            foreach (var property in type.Properties)
            {
                if (property.CustomAttributes.Any(attribute => attribute.AttributeType.Resolve() == encapsulatedAttributeType))
                {
                    var dtoField = new FieldDefinition(
                        string.Format("<{0}>k__BackingField", property.Name),
                        FieldAttributes.Private,
                        property.PropertyType);
                    dtoField.CustomAttributes.Add(new CustomAttribute(compilerGeneratedAttributeConstructorReference));
                    dtoField.DeclaringType = dtoType;
                    dtoType.Fields.Add(dtoField);
                    var dtoProperty = new PropertyDefinition(property.Name, PropertyAttributes.None, property.PropertyType);
                    dtoProperty.DeclaringType = dtoType;

                    var dtoPropertyGetter = new MethodDefinition(
                        string.Format("get_{0}", property.Name),
                        MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                        property.PropertyType);
                    dtoPropertyGetter.DeclaringType = dtoType;
                    ilProcessor = dtoPropertyGetter.Body.GetILProcessor();
                    ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
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
