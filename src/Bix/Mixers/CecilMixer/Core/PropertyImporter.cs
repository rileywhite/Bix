using Mono.Cecil;
using System;
using System.Linq;
using System.Reflection;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class PropertyImporter : MemberImporterBase<PropertyInfo, PropertyDefinition>
    {
        public override PropertyDefinition GetMemberDefinition(PropertyInfo memberInfo)
        {
            var sourceType = this.ReferencingModule.Import(memberInfo.DeclaringType).Resolve();
            return sourceType.Properties.First(property => property.Name == memberInfo.Name);
        }
    }
}
