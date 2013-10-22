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
        private TypeReference VoidTypeReference { get; set; }
        private TypeReference IEncapsulatesTypeReference { get; set; }
        private TypeReference IMixesTypeReference { get; set; }
        private TypeReference ISerializableTypeReference { get; set; }
        private TypeReference ObjectTypeReference { get; set; }
        private TypeReference SerializationInfoTypeReference { get; set; }
        private TypeReference StreamingContextTypeReference { get; set; }
        private TypeReference ReadOnlyCollectionOfIMixerTypeReference { get; set; }

        private TypeReference EncapsulatesAttributeType { get; set; }
        private TypeDefinition EncapsulatedAttributeType { get; set; }

        private MethodReference ObjectConstructorReference { get; set; }
        private MethodReference CompilerGeneratedAttributeConstructorReference { get; set; }

        public void AddEncapsulation(string assemblyPath)
        {
            var typeModule = ModuleDefinition.ReadModule(assemblyPath);

            Initialize(typeModule);

            var originalTypes = new List<TypeDefinition>(typeModule.Types);
            foreach (var type in originalTypes)
            {
                var encapsulatesAttribute = type.CustomAttributes.SingleOrDefault(customAttribute => customAttribute.AttributeType.Resolve() == this.EncapsulatesAttributeType);
                if (encapsulatesAttribute != null)
                {
                    AddEncapsulation(typeModule, type);
                    type.CustomAttributes.Remove(encapsulatesAttribute);
                }
            }

            typeModule.Write(assemblyPath, new WriterParameters { SymbolWriterProvider = new PdbWriterProvider() });
        }

        private void Initialize(ModuleDefinition typeModule)
        {
            this.VoidTypeReference = typeModule.Import(typeof(void));
            this.IEncapsulatesTypeReference = typeModule.Import(typeof(IEncapsulates));
            this.IMixesTypeReference = typeModule.Import(typeof(IMixes));
            this.ISerializableTypeReference = typeModule.Import(typeof(ISerializable));
            this.ObjectTypeReference = typeModule.Import(typeof(object));
            this.SerializationInfoTypeReference = typeModule.Import(typeof(SerializationInfo));
            this.StreamingContextTypeReference = typeModule.Import(typeof(StreamingContext));
            this.ReadOnlyCollectionOfIMixerTypeReference = typeModule.Import(typeof(ReadOnlyCollection<IMixer>));

            this.EncapsulatesAttributeType = typeModule.Import(typeof(EncapsulatesAttribute)).Resolve();
            this.EncapsulatedAttributeType = typeModule.Import(typeof(EncapsulatedAttribute)).Resolve();

            this.ObjectConstructorReference = typeModule.Import(typeof(object).GetConstructor(new Type[0]));
            this.CompilerGeneratedAttributeConstructorReference = typeModule.Import(typeof(CompilerGeneratedAttribute).GetConstructor(new Type[0]));
        }

        private void AddEncapsulation(ModuleDefinition typeModule, TypeDefinition type)
        {
            this.AddIMixes(typeModule, type);
            this.AddISerializable(typeModule, type);
            this.AddIEncapsulates(typeModule, type);
        }

        private void AddIMixes(ModuleDefinition typeModule, TypeDefinition type)
        {
            type.Interfaces.Add(this.IMixesTypeReference);

            var mixersField = new FieldDefinition(
                "mixers",
                FieldAttributes.Private | FieldAttributes.SpecialName,
                this.ReadOnlyCollectionOfIMixerTypeReference);
            mixersField.CustomAttributes.Add(new CustomAttribute(this.CompilerGeneratedAttributeConstructorReference));
            type.Fields.Add(mixersField);

            var mixersGetter = new MethodDefinition(
                "Bix.Mix.IMixes.get_Mixers",
                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.SpecialName,
                this.ReadOnlyCollectionOfIMixerTypeReference);
            mixersGetter.Overrides.Add(typeModule.Import(typeof(IMixes).GetProperty("Mixers").GetGetMethod()));
            mixersGetter.Body.Variables.Add(new VariableDefinition(this.ReadOnlyCollectionOfIMixerTypeReference));
            mixersGetter.Body.InitLocals = true;
            var ilProcessor = mixersGetter.Body.GetILProcessor();
            ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
            ilProcessor.Append(Instruction.Create(OpCodes.Ldfld, mixersField));
            ilProcessor.Append(Instruction.Create(OpCodes.Stloc_0));
            Instruction weird = Instruction.Create(OpCodes.Ldloc_0);
            ilProcessor.Append(Instruction.Create(OpCodes.Br_S, weird));
            ilProcessor.Append(weird);
            ilProcessor.Append(Instruction.Create(OpCodes.Ret));
            type.Methods.Add(mixersGetter);

            var mixersProperty = new PropertyDefinition(
                "Bix.Mix.IMixes.Mixers",
                PropertyAttributes.None,
                this.ReadOnlyCollectionOfIMixerTypeReference);
            mixersProperty.CustomAttributes.Add(new CustomAttribute(this.CompilerGeneratedAttributeConstructorReference));
            mixersProperty.GetMethod = mixersGetter;
            type.Properties.Add(mixersProperty);
        }

        private void AddISerializable(ModuleDefinition typeModule, TypeDefinition type)
        {
            type.Interfaces.Add(this.ISerializableTypeReference);

            var getObjectDataMethod = new MethodDefinition(
                "System.Runtime.Serialization.ISerializable.GetObjectData",
                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                this.VoidTypeReference);
            getObjectDataMethod.Parameters.Add(new ParameterDefinition("info", ParameterAttributes.None, this.SerializationInfoTypeReference));
            getObjectDataMethod.Parameters.Add(new ParameterDefinition("context", ParameterAttributes.None, this.StreamingContextTypeReference));
            getObjectDataMethod.Overrides.Add(typeModule.Import(typeof(ISerializable).GetMethod("GetObjectData", new Type[] { typeof(SerializationInfo), typeof(StreamingContext) })));
            var ilProcessor = getObjectDataMethod.Body.GetILProcessor();
            ilProcessor.Append(Instruction.Create(OpCodes.Ret));
            getObjectDataMethod.DeclaringType = type;
            type.Methods.Add(getObjectDataMethod);

            var serializationConstructor = new MethodDefinition(
                ".ctor",
                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                this.VoidTypeReference);
            serializationConstructor.Parameters.Add(new ParameterDefinition("info", ParameterAttributes.None, this.SerializationInfoTypeReference));
            serializationConstructor.Parameters.Add(new ParameterDefinition("context", ParameterAttributes.None, this.StreamingContextTypeReference));
            ilProcessor = serializationConstructor.Body.GetILProcessor();
            ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
            ilProcessor.Append(Instruction.Create(OpCodes.Call, this.ObjectConstructorReference));
            ilProcessor.Append(Instruction.Create(OpCodes.Ret));
            serializationConstructor.DeclaringType = type;
            type.Methods.Add(serializationConstructor);
        }

        private void AddIEncapsulates(ModuleDefinition typeModule, TypeDefinition type)
        {
            type.Interfaces.Add(this.IEncapsulatesTypeReference);

            // create inner DTO
            var dtoType = new TypeDefinition(string.Empty, "Dto", TypeAttributes.NestedPublic | TypeAttributes.Class, this.ObjectTypeReference);
            var dtoConstructor = new MethodDefinition(
                ".ctor",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                this.VoidTypeReference);
            var ilProcessor = dtoConstructor.Body.GetILProcessor();
            ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
            ilProcessor.Append(Instruction.Create(OpCodes.Call, this.ObjectConstructorReference));
            ilProcessor.Append(Instruction.Create(OpCodes.Ret));
            dtoConstructor.DeclaringType = dtoType;
            dtoType.Methods.Add(dtoConstructor);
            foreach (var property in type.Properties)
            {
                var encapsulatedAttribute = property.CustomAttributes.SingleOrDefault(customAttribute => customAttribute.AttributeType.Resolve() == this.EncapsulatedAttributeType);
                if (encapsulatedAttribute != null)
                {
                    var dtoField = new FieldDefinition(
                        string.Format("<{0}>k__BackingField", property.Name),
                        FieldAttributes.Private,
                        property.PropertyType);
                    dtoField.CustomAttributes.Add(new CustomAttribute(this.CompilerGeneratedAttributeConstructorReference));
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

                    property.CustomAttributes.Remove(encapsulatedAttribute);
                }
            }

            type.NestedTypes.Add(dtoType);
        }
    }
}
