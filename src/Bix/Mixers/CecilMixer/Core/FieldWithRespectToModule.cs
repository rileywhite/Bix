using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class FieldWithRespectToModule
        : MemberWithRespectToModuleBase<FieldInfo, FieldDefinition>
    {
        public FieldWithRespectToModule(FieldInfo field, ModuleDefinition referencingModule)
            : base(field, referencingModule) { }

        private FieldImporter memberImporter = new FieldImporter();
        public override IMemberImporter<FieldInfo, FieldDefinition> MemberImporter
        {
            get { return this.memberImporter; }
        }
    }
}
