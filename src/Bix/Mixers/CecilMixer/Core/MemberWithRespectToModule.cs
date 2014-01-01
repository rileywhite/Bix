using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class MemberWithRespectToModule<TMemberInfo, TMemberReference, TMemberDefinition>
        where TMemberInfo : MemberInfo
        where TMemberReference : MemberReference
        where TMemberDefinition : IMemberDefinition, Mono.Cecil.ICustomAttributeProvider
    {
        public MemberWithRespectToModule(TMemberInfo member, ModuleDefinition referencingModule)
        {
            Contract.Requires(member != null);
            Contract.Requires(referencingModule != null);
            Contract.Ensures(this.Member != null);
            Contract.Ensures(this.ReferencingModule != null);
            Contract.Ensures(this.CecilMemberDefinition != null);

            this.Member = member;
            this.ReferencingModule = referencingModule;
            var cecilMemberReference = referencingModule.Import((dynamic)member);
            this.CecilMemberDefinition = ((dynamic)cecilMemberReference).Resolve();
        }

        public ModuleDefinition ReferencingModule { get; private set; }

        public TMemberInfo Member { get; private set; }

        public TMemberDefinition CecilMemberDefinition { get; private set; }
    }
}
