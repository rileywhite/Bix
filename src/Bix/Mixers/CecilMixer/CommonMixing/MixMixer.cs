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

            new InterfaceMixCommand<IMixes, MixesSource>(target).Mix();
        }

        private void AddISerializable(TypeDefinition target)
        {
            Contract.Requires(target != null);
            Contract.Requires(target.Module != null);

            new InterfaceMixCommand<ISerializable, SerializableSource>(target).Mix();
        }
    }
}
