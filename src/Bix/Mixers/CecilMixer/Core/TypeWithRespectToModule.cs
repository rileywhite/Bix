using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class TypeWithRespectToModule : MemberWithRespectToModuleBase<Type, TypeDefinition>
    {
        public TypeWithRespectToModule(Type type, ModuleDefinition referencingModule)
            : base(type, referencingModule) { }

        private TypeImporter memberImporter = new TypeImporter();
        public override IMemberImporter<Type, TypeDefinition> MemberImporter
        {
            get { return this.memberImporter; }
        }
    }
}
