using Mono.Cecil;
using System;
using System.Reflection;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class FieldWithRespectToModule
        : MemberWithRespectToModule<FieldInfo, FieldReference, FieldDefinition>
    {
        public FieldWithRespectToModule(FieldInfo field, ModuleDefinition referencingModule)
            : base(field, referencingModule) { }
    }
}
