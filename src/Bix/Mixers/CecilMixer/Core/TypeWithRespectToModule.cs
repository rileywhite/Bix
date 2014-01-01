using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class TypeWithRespectToModule : MemberWithRespectToModule<Type, TypeReference, TypeDefinition>
    {
        public TypeWithRespectToModule(Type type, ModuleDefinition referencingModule)
            : base(type, referencingModule) { }
    }
}
