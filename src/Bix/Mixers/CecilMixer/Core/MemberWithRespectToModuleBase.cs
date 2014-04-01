using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.CecilMixer.Core
{
    internal abstract class MemberWithRespectToModuleBase<TMemberInfo, TMemberDefinition>
        where TMemberInfo : MemberInfo
        where TMemberDefinition : MemberReference, IMemberDefinition, Mono.Cecil.ICustomAttributeProvider
    {
        public MemberWithRespectToModuleBase(
            TMemberInfo member,
            ModuleDefinition referencingModule)
        {
            Contract.Requires(member != null);
            Contract.Requires(referencingModule != null);
            Contract.Ensures(this.MemberInfo != null);
            Contract.Ensures(this.ReferencingModule != null);
            Contract.Ensures(this.MemberImporter != null);
            Contract.Ensures(this.MemberDefinition != null);

            this.MemberInfo = member;
            this.ReferencingModule = referencingModule;
            this.MemberImporter.ReferencingModule = referencingModule;
        }

        public ModuleDefinition ReferencingModule { get; private set; }

        public abstract IMemberImporter<TMemberInfo, TMemberDefinition> MemberImporter { get; }

        public TMemberInfo MemberInfo { get; private set; }

        private TMemberDefinition memberDefinition;
        public TMemberDefinition MemberDefinition
        {
            get
            {
                if(this.memberDefinition == null)
                {
                    this.memberDefinition = this.MemberImporter.GetMemberDefinition(this.MemberInfo);
                }
                return this.memberDefinition;
            }
        }
    }
}
