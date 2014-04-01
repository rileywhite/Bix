using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class TypeImporter : MemberImporterBase<Type, TypeDefinition>
    {
        public override TypeDefinition GetMemberDefinition(Type memberInfo)
        {
            return this.ReferencingModule.Import(memberInfo).Resolve();
        }
    }
}
