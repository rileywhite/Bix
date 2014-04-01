using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class PropertyMixer : MemberMixerBase<PropertyInfo, PropertyDefinition, PropertyWithRespectToModule>
    {
        public PropertyMixer(PropertyDefinition target, PropertyWithRespectToModule source)
            : base(target, source)
        {
            Contract.Requires(target != null);
            Contract.Requires(source != null);
        }


        public override void Mix()
        {
            Contract.Assert(this.Target.DeclaringType != null);
            Contract.Assert(this.Target.Name == this.Source.MemberDefinition.Name);

            this.Target.Attributes = this.Source.MemberDefinition.Attributes;
            this.Target.Constant = this.Source.MemberDefinition.Constant;
            this.Target.HasConstant = this.Source.MemberDefinition.HasConstant;
            this.Target.HasDefault = this.Source.MemberDefinition.HasDefault;
            this.Target.HasThis = this.Source.MemberDefinition.HasThis;
            this.Target.IsRuntimeSpecialName = this.Source.MemberDefinition.IsRuntimeSpecialName;
            this.Target.IsSpecialName = this.Source.MemberDefinition.IsSpecialName;

            // TODO process this type in case it is a mixed type
            this.Target.PropertyType = this.Source.ReferencingModule.Import(this.Source.MemberDefinition.PropertyType);

            for (int i = 0; i < this.Source.MemberDefinition.OtherMethods.Count; i++)
            {
                this.Target.OtherMethods.Add(null);
            }

            foreach(var method in this.Target.DeclaringType.Methods)
            {
                if (this.Source.MemberDefinition.GetMethod != null &&
                    this.Target.GetMethod == null &&
                    method.SignatureEquals(this.Source.MemberDefinition.GetMethod))
                {
                    this.Target.GetMethod = method;
                }

                if (this.Source.MemberDefinition.SetMethod != null &&
                    this.Target.SetMethod == null &&
                    method.SignatureEquals(this.Source.MemberDefinition.SetMethod))
                {
                    this.Target.SetMethod = method;
                }

                for (int i = 0; i < this.Source.MemberDefinition.OtherMethods.Count; i++)
                {
                    if (this.Target.OtherMethods[i] != null &&
                        this.Target.OtherMethods[i] == null &&
                        method.SignatureEquals(this.Source.MemberDefinition.OtherMethods[i]))
                    {
                        this.Target.OtherMethods[i] = method;
                    }
                }
            }
            this.Target.MetadataToken = this.Source.MemberDefinition.MetadataToken;
            //this.Target.CustomAttributes = this.Source.MemberDefinition;
            //this.Target.Parameters = this.Source.MemberDefinition;

            this.IsMixed = true;

            Contract.Assert(this.Target.SignatureEquals(this.Source.MemberDefinition));
            Contract.Assert((this.Target.GetMethod == null) == (this.Source.MemberDefinition.GetMethod == null));
            Contract.Assert((this.Target.SetMethod == null) == (this.Source.MemberDefinition.SetMethod == null));
            for (int i = 0; i < this.Source.MemberDefinition.OtherMethods.Count; i++)
            {
                Contract.Assert(
                    this.Source.MemberDefinition.OtherMethods[i] == null ||
                    this.Target.OtherMethods[i].FullName == this.Source.MemberDefinition.OtherMethods[i].FullName);
            }
        }
    }
}
