using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class MethodWithRespectToModule
        : MemberWithRespectToModuleBase<MethodInfo, MethodDefinition>
    {
        public MethodWithRespectToModule(MethodInfo method, ModuleDefinition referencingModule)
            : base(method, referencingModule) { }

        private MethodImporter memberImporter = new MethodImporter();
        public override IMemberImporter<MethodInfo, MethodDefinition> MemberImporter
        {
            get { return this.memberImporter; }
        }
    }
}
