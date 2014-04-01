using Mono.Cecil;
using System;
using System.Reflection;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class MethodImporter : MemberImporterBase<MethodInfo, MethodDefinition>
    {
        public override MethodDefinition GetMemberDefinition(MethodInfo memberInfo)
        {
            return this.ReferencingModule.Import(memberInfo).Resolve();
        }
    }
}
