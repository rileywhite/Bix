using Bix.Mix;
using Bix.Mix.Encapsulate;
using Bix.Mixers.CecilMixer.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ParameterInfo = System.Reflection.ParameterInfo;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;

namespace Bix.Mixers.CecilMixer.CommonMixing
{
    internal class MixMixer
    {
        public void AddMixing(ModuleDefinition targetModule)
        {
            var objectType = targetModule.Import(typeof(object)).Resolve();
            var mixesAttributeBaseType = targetModule.Import(typeof(MixesAttributeBase)).Resolve();

            var originalTypes = new List<TypeDefinition>(targetModule.Types);
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
                    this.AddIMixes(type);
                    this.AddISerializable(type);
                }
            }
        }

        private void AddIMixes(TypeDefinition target)
        {
            Contract.Requires(target != null);
            Contract.Requires(target.Module != null);

            target.Interfaces.Add(target.Module.Import(typeof(IMixes)));
            var mixersProperty = target.ImplementAutoPropertyExplicitly(typeof(IMixes).GetProperty("Mixers"));
            mixersProperty.MarkAsCompilerGenerated();
        }

        private void AddISerializable(TypeDefinition target)
        {
            Contract.Requires(target != null);
            Contract.Requires(target.Module != null);

            var targetModule = target.Module;

            target.Interfaces.Add(targetModule.Import(typeof(ISerializable)));

            var voidTypeReference = targetModule.Import(typeof(void));
            var serializationInfoTypeReference = targetModule.Import(typeof(SerializationInfo));
            var streamingContextTypeReference = targetModule.Import(typeof(StreamingContext));

            target.ImplementMethodExplicitly(
                typeof(ISerializable).GetMethod("GetObjectData", new Type[] { typeof(SerializationInfo), typeof(StreamingContext) }),
                ilProcessor =>
                {
                    ilProcessor.Append(Instruction.Create(OpCodes.Ret));
                });

            target.AddPrivateConstructor(
                ilProcessor =>
                {
                    ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
                    ilProcessor.Append(Instruction.Create(OpCodes.Call, targetModule.ImportConstructor(typeof(object))));
                    ilProcessor.Append(Instruction.Create(OpCodes.Ret));
                },
                new ParameterDefinition[] { new ParameterDefinition("info", ParameterAttributes.None, serializationInfoTypeReference), new ParameterDefinition("context", ParameterAttributes.None, streamingContextTypeReference) });
        }
    }
}
