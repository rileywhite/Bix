using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class MethodMixer : MemberMixerBase<MethodInfo, MethodDefinition, MethodWithRespectToModule>
    {
        public MethodMixer(MethodDefinition target, MethodWithRespectToModule source)
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
            this.Target.CallingConvention = this.Source.MemberDefinition.CallingConvention;
            this.Target.ExplicitThis = this.Source.MemberDefinition.ExplicitThis;
            this.Target.HasSecurity = this.Source.MemberDefinition.HasSecurity;
            this.Target.HasThis = this.Source.MemberDefinition.HasThis;
            this.Target.ImplAttributes = this.Source.MemberDefinition.ImplAttributes;
            this.Target.IsAbstract = this.Source.MemberDefinition.IsAbstract;
            this.Target.IsAddOn = this.Source.MemberDefinition.IsAddOn;
            this.Target.IsAssembly = this.Source.MemberDefinition.IsAssembly;
            this.Target.IsCheckAccessOnOverride = this.Source.MemberDefinition.IsCheckAccessOnOverride;
            this.Target.IsCompilerControlled = this.Source.MemberDefinition.IsCompilerControlled;
            this.Target.IsFamily = this.Source.MemberDefinition.IsFamily;
            this.Target.IsFamilyAndAssembly = this.Source.MemberDefinition.IsFamilyAndAssembly;
            this.Target.IsFamilyOrAssembly = this.Source.MemberDefinition.IsFamilyOrAssembly;
            this.Target.IsFinal = this.Source.MemberDefinition.IsFinal;
            this.Target.IsFire = this.Source.MemberDefinition.IsFire;
            this.Target.IsForwardRef = this.Source.MemberDefinition.IsForwardRef;
            this.Target.IsGetter = this.Source.MemberDefinition.IsGetter;
            this.Target.IsHideBySig = this.Source.MemberDefinition.IsHideBySig;
            this.Target.IsIL = this.Source.MemberDefinition.IsIL;
            this.Target.IsInternalCall = this.Source.MemberDefinition.IsInternalCall;
            this.Target.IsManaged = this.Source.MemberDefinition.IsManaged;
            this.Target.IsNative = this.Source.MemberDefinition.IsNative;
            this.Target.IsNewSlot = this.Source.MemberDefinition.IsNewSlot;
            this.Target.IsOther = this.Source.MemberDefinition.IsOther;
            this.Target.IsPInvokeImpl = this.Source.MemberDefinition.IsPInvokeImpl;
            this.Target.IsPreserveSig = this.Source.MemberDefinition.IsPreserveSig;
            this.Target.IsPrivate = this.Source.MemberDefinition.IsPrivate;
            this.Target.IsPublic = this.Source.MemberDefinition.IsPublic;
            this.Target.IsRemoveOn = this.Source.MemberDefinition.IsRemoveOn;
            this.Target.IsReuseSlot = this.Source.MemberDefinition.IsReuseSlot;
            this.Target.IsRuntime = this.Source.MemberDefinition.IsRuntime;
            this.Target.IsRuntimeSpecialName = this.Source.MemberDefinition.IsRuntimeSpecialName;
            this.Target.IsSetter = this.Source.MemberDefinition.IsSetter;
            this.Target.IsSpecialName = this.Source.MemberDefinition.IsSpecialName;
            this.Target.IsStatic = this.Source.MemberDefinition.IsStatic;
            this.Target.IsSynchronized = this.Source.MemberDefinition.IsSynchronized;
            this.Target.IsUnmanaged = this.Source.MemberDefinition.IsUnmanaged;
            this.Target.IsUnmanagedExport = this.Source.MemberDefinition.IsUnmanagedExport;
            this.Target.IsVirtual = this.Source.MemberDefinition.IsVirtual;
            this.Target.NoInlining = this.Source.MemberDefinition.NoInlining;
            this.Target.NoOptimization = this.Source.MemberDefinition.NoOptimization;
            this.Target.SemanticsAttributes = this.Source.MemberDefinition.SemanticsAttributes;

            // TODO look more closely
            this.Target.MetadataToken = this.Source.MemberDefinition.MetadataToken;
            if (this.Source.MemberDefinition.PInvokeInfo != null)
            {
                this.Target.PInvokeInfo = new PInvokeInfo(
                    this.Source.MemberDefinition.PInvokeInfo.Attributes,
                    this.Source.MemberDefinition.PInvokeInfo.EntryPoint,
                    this.Source.MemberDefinition.PInvokeInfo.Module);
            }
            //this.Target.MethodReturnType = new MethodReturnType(this.Target)
            //{
            //    Attributes = this.Source.MemberDefinition.MethodReturnType.Attributes,
            //    Constant = this.Source.MemberDefinition.MethodReturnType.Constant,
            //    // CustomAttributes
            //    HasConstant = this.Source.MemberDefinition.MethodReturnType.HasConstant,
            //    HasDefault = this.Source.MemberDefinition.MethodReturnType.HasDefault,
            //    HasFieldMarshal = this.Source.MemberDefinition.MethodReturnType.HasFieldMarshal,
            //    MarshalInfo = this.Source.MemberDefinition.MethodReturnType.MarshalInfo,
            //    MetadataToken = this.Source.MemberDefinition.MethodReturnType.MetadataToken,
            //    ReturnType = this.Source.ReferencingModule.Import(this.Source.MemberDefinition.MethodReturnType.ReturnType)
            //};
            this.Target.ReturnType = this.Source.ReferencingModule.Import(this.Source.MemberDefinition.ReturnType);

            foreach(var parameter in this.Source.MemberInfo.GetParameters().ToParameterDefinitionsForModule(this.Source.ReferencingModule))
            {
                // TODO process parameter type in case they are mixed types
                this.Target.Parameters.Add(parameter);
            }

            //this.Target.Body = this.Source.MemberDefinition;
            //this.Target.CustomAttributes = this.Source.MemberDefinition;
            //this.Target.GenericParameters = this.Source.MemberDefinition;
            //this.Target.SecurityDeclarations = this.Source.MemberDefinition;

            this.IsMixed = true;
            Contract.Assert(this.Target.FullName == this.Source.MemberDefinition.FullName);
        }
    }
}
