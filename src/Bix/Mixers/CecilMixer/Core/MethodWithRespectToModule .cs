using Mono.Cecil;
using System;
using System.Reflection;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class MethodWithRespectToModule
        : MemberWithRespectToModule<MethodInfo, MethodReference, MethodDefinition>
    {
        public MethodWithRespectToModule(MethodInfo method, ModuleDefinition referencingModule)
            : base(method, referencingModule) { }
    }
}
