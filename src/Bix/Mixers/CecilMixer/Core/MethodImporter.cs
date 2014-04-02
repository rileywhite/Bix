using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class MethodImporter : MemberImporterBase<MethodInfo, MethodDefinition>
    {
        public MethodImporter() { }

        public MethodImporter(ModuleDefinition referencingModule)
            : base(referencingModule)
        {
            Contract.Requires(referencingModule != null);
            Contract.Ensures(this.ReferencingModule != null);
        }

        public override MethodDefinition GetMemberDefinition(MethodInfo memberInfo)
        {
            return this.ReferencingModule.Import(memberInfo).Resolve();
        }
    }
}
