using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class PropertyWithRespectToModule
        : MemberWithRespectToModuleBase<PropertyInfo, PropertyDefinition>
    {
        public PropertyWithRespectToModule(PropertyInfo property, ModuleDefinition referencingModule)
            : base(property, referencingModule) { }

        private PropertyImporter memberImporter = new PropertyImporter();
        public override IMemberImporter<PropertyInfo, PropertyDefinition> MemberImporter
        {
            get { return this.memberImporter; }
        }
    }
}
