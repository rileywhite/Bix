using Mono.Cecil;
using System;
using System.Reflection;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class FieldImporter : MemberImporterBase<FieldInfo, FieldDefinition>
    {
        public override FieldDefinition GetMemberDefinition(FieldInfo memberInfo)
        {
            return this.ReferencingModule.Import(memberInfo).Resolve();
        }
    }
}
