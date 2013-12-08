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
        internal void AddMixing(string modulePath, ModuleDefinition typeModule)
        {
            var objectType = typeModule.Import(typeof(object)).Resolve();
            var mixesAttributeBaseType = typeModule.Import(typeof(MixesAttributeBase)).Resolve();

            var originalTypes = new List<TypeDefinition>(typeModule.Types);
            foreach (var type in originalTypes)
            {
                if(type.CustomAttributes.Any(
                    customAttribute =>
                    {
                        var attributeType = customAttribute.AttributeType.Resolve();
                        while (attributeType != objectType &&
                            attributeType != mixesAttributeBaseType &&
                            attributeType.BaseType != null)
                        {
                            attributeType = attributeType.BaseType.Resolve();
                        }
                        return attributeType == mixesAttributeBaseType;
                    }))
                {
                    this.AddIMixes(typeModule, type);
                    this.AddISerializable(typeModule, type);
                }
            }

            typeModule.Write(modulePath, new WriterParameters { SymbolWriterProvider = new PdbWriterProvider() });
        }

        private void AddIMixes(ModuleDefinition typeModule, TypeDefinition type)
        {
            type.Interfaces.Add(typeModule.Import(typeof(IMixes)));
            var mixersProperty = type.ImplementAutoPropertyExplicitly(typeof(IMixes).GetProperty("Mixers"));
            mixersProperty.MarkAsCompilerGenerated();
        }

        private void AddISerializable(ModuleDefinition typeModule, TypeDefinition type)
        {
            type.Interfaces.Add(typeModule.Import(typeof(ISerializable)));

            var voidTypeReference = typeModule.Import(typeof(void));
            var serializationInfoTypeReference = typeModule.Import(typeof(SerializationInfo));
            var streamingContextTypeReference = typeModule.Import(typeof(StreamingContext));

            var getObjectDataMethod = type.AddMethod(
                "System.Runtime.Serialization.ISerializable.GetObjectData",
                voidTypeReference,
                ilProcessor =>
                {
                    ilProcessor.Append(Instruction.Create(OpCodes.Ret));
                },
                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final);
            getObjectDataMethod.Parameters.Add(new ParameterDefinition("info", ParameterAttributes.None, serializationInfoTypeReference));
            getObjectDataMethod.Parameters.Add(new ParameterDefinition("context", ParameterAttributes.None, streamingContextTypeReference));
            getObjectDataMethod.Overrides.Add(typeModule.Import(typeof(ISerializable).GetMethod("GetObjectData", new Type[] { typeof(SerializationInfo), typeof(StreamingContext) })));

            var serializationConstructor = type.AddMethod(
                ".ctor",
                voidTypeReference,
                ilProcessor =>
                {
                    ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
                    ilProcessor.Append(Instruction.Create(OpCodes.Call, typeModule.ImportConstructor(typeof(object))));
                    ilProcessor.Append(Instruction.Create(OpCodes.Ret));
                },
                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
            serializationConstructor.Parameters.Add(new ParameterDefinition("info", ParameterAttributes.None, serializationInfoTypeReference));
            serializationConstructor.Parameters.Add(new ParameterDefinition("context", ParameterAttributes.None, streamingContextTypeReference));
        }
    }
}
