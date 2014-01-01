using Mono.Cecil;
using System;
using System.Reflection;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class PropertyWithRespectToModule
        : MemberWithRespectToModule<PropertyInfo, PropertyReference, PropertyDefinition>
    {
        public PropertyWithRespectToModule(PropertyInfo property, ModuleDefinition referencingModule)
            : base(property, referencingModule) { }
    }
}
