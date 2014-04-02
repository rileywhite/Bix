using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class MethodWithRespectToModule
        : MemberWithRespectToModuleBase<MethodInfo, MethodDefinition>
    {
        public MethodWithRespectToModule(RootContext rootContext, MethodInfo method, ModuleDefinition referencingModule)
            : base(rootContext, method, referencingModule)
        {
            Contract.Requires(rootContext != null);
            Contract.Ensures(this.RootContext != null);
        }

        private MethodImporter memberImporter = new MethodImporter();
        public override IMemberImporter<MethodInfo, MethodDefinition> MemberImporter
        {
            get { return this.memberImporter; }
        }
    }
}
