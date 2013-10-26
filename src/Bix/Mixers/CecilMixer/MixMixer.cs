using Bix.Mix;
using Bix.Mix.Encapsulate;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Bix.Mixers.CecilMixer
{
    internal class MixMixer
    {
        private TypeReference VoidTypeReference { get; set; }
        private TypeReference IMixesTypeReference { get; set; }
        private TypeReference ISerializableTypeReference { get; set; }
        private TypeReference SerializationInfoTypeReference { get; set; }
        private TypeReference StreamingContextTypeReference { get; set; }
        private TypeReference ReadOnlyCollectionOfIMixerTypeReference { get; set; }

        private TypeReference EncapsulatesAttributeType { get; set; }

        private MethodReference ObjectConstructorReference { get; set; }
        private MethodReference CompilerGeneratedAttributeConstructorReference { get; set; }

        internal void AddMixing(string modulePath, ModuleDefinition typeModule)
        {
            Initialize(typeModule);

            var originalTypes = new List<TypeDefinition>(typeModule.Types);
            foreach (var type in originalTypes)
            {
                var encapsulatesAttribute = type.CustomAttributes.SingleOrDefault(customAttribute => customAttribute.AttributeType.Resolve() == this.EncapsulatesAttributeType);
                if (encapsulatesAttribute != null)
                {
                    this.AddIMixes(typeModule, type);
                    this.AddISerializable(typeModule, type);
                }
            }

            typeModule.Write(modulePath, new WriterParameters { SymbolWriterProvider = new PdbWriterProvider() });
        }

        private void Initialize(ModuleDefinition typeModule)
        {
            this.VoidTypeReference = typeModule.Import(typeof(void));
            this.IMixesTypeReference = typeModule.Import(typeof(IMixes));
            this.ISerializableTypeReference = typeModule.Import(typeof(ISerializable));
            this.SerializationInfoTypeReference = typeModule.Import(typeof(SerializationInfo));
            this.StreamingContextTypeReference = typeModule.Import(typeof(StreamingContext));
            this.ReadOnlyCollectionOfIMixerTypeReference = typeModule.Import(typeof(ReadOnlyCollection<IMixer>));

            this.EncapsulatesAttributeType = typeModule.Import(typeof(EncapsulatesAttribute)).Resolve();

            this.ObjectConstructorReference = typeModule.Import(typeof(object).GetConstructor(new Type[0]));
            this.CompilerGeneratedAttributeConstructorReference = typeModule.Import(typeof(CompilerGeneratedAttribute).GetConstructor(new Type[0]));
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
    }
}
