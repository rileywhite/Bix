using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class FieldMixer : MemberMixerBase<FieldInfo, FieldDefinition, FieldWithRespectToModule>
    {
        public FieldMixer(FieldDefinition target, FieldWithRespectToModule source)
            : base(target, source) { }

        public override void Mix()
        {
            this.IsMixed = true;

            this.Target.Attributes = this.Source.MemberDefinition.Attributes;
            this.Target.Constant = this.Source.MemberDefinition.Constant;
            this.Target.HasConstant = this.Source.MemberDefinition.HasConstant;
            this.Target.HasDefault = this.Source.MemberDefinition.HasDefault;
            this.Target.IsAssembly = this.Source.MemberDefinition.IsAssembly;
            this.Target.IsCompilerControlled = this.Source.MemberDefinition.IsCompilerControlled;
            this.Target.IsFamily = this.Source.MemberDefinition.IsFamily;
            this.Target.IsFamilyAndAssembly = this.Source.MemberDefinition.IsFamilyAndAssembly;
            this.Target.IsFamilyOrAssembly = this.Source.MemberDefinition.IsFamilyOrAssembly;
            this.Target.IsInitOnly = this.Source.MemberDefinition.IsInitOnly;
            this.Target.IsLiteral = this.Source.MemberDefinition.IsLiteral;
            this.Target.IsNotSerialized = this.Source.MemberDefinition.IsNotSerialized;
            this.Target.IsPInvokeImpl = this.Source.MemberDefinition.IsPInvokeImpl;
            this.Target.IsPrivate = this.Source.MemberDefinition.IsPrivate;
            this.Target.IsPublic = this.Source.MemberDefinition.IsPublic;
            this.Target.IsRuntimeSpecialName = this.Source.MemberDefinition.IsRuntimeSpecialName;
            this.Target.IsSpecialName = this.Source.MemberDefinition.IsSpecialName;
            this.Target.IsStatic = this.Source.MemberDefinition.IsStatic;
            this.Target.Name = this.Source.MemberDefinition.Name;
            this.Target.Offset = this.Source.MemberDefinition.Offset;

            if (this.Source.MemberDefinition.MarshalInfo == null)
            {
                this.Target.MarshalInfo = null;
            }
            else
            {
                this.Target.MarshalInfo = new MarshalInfo(this.Source.MemberDefinition.MarshalInfo.NativeType);
            }

            if (this.Source.MemberDefinition.InitialValue != null)
            {
                var initialValue = new byte[this.Source.MemberDefinition.InitialValue.LongLength];
                this.Source.MemberDefinition.InitialValue.CopyTo(initialValue, 0);
                this.Target.InitialValue = initialValue;
            }

            this.Target.MetadataToken = new MetadataToken(
                this.Source.MemberDefinition.MetadataToken.TokenType,
                this.Source.MemberDefinition.MetadataToken.RID);

            // TODO process type (either import or redirect to type within current module)
            this.Target.FieldType = this.Source.RootImport(this.Source.MemberDefinition.FieldType);

            // TODO copy custom attributes and either import or redirect to type within current module
            foreach(var source in this.Source.MemberDefinition.CustomAttributes)
            {
                this.Target.CustomAttributes.Add(new CustomAttribute(this.Source.ReferencingModule.Import(source.Constructor), source.GetBlob()));
            }
        }
    }
}
