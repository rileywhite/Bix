using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class TypeMixer : MemberMixerBase<Type, TypeDefinition, TypeWithRespectToModule>
    {
        public TypeMixer(TypeDefinition target, TypeWithRespectToModule source)
            : base(target, source)
        {
            Contract.Requires(target != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.Mixers != null);
            this.Mixers = new List<IMemberMixer>();
        }

        private List<IMemberMixer> Mixers { get; set; }

        private bool IsScaffoldingCompleted { get; set; }

        public override void Mix()
        {
            this.PopulateMixersWithScaffolding();
            this.CopyTypeData();
            this.Mixers.Mix();
            this.IsMixed = true;
        }

        private void CopyTypeData()
        {
            this.Target.Attributes = this.Source.MemberDefinition.Attributes;
            this.Target.ClassSize = this.Source.MemberDefinition.ClassSize;
            this.Target.HasSecurity = this.Source.MemberDefinition.HasSecurity;
            this.Target.IsAbstract = this.Source.MemberDefinition.IsAbstract;
            this.Target.IsAnsiClass = this.Source.MemberDefinition.IsAnsiClass;
            this.Target.IsAutoClass = this.Source.MemberDefinition.IsAutoClass;
            this.Target.IsAutoLayout = this.Source.MemberDefinition.IsAutoLayout;
            this.Target.IsBeforeFieldInit = this.Source.MemberDefinition.IsBeforeFieldInit;
            this.Target.IsClass = this.Source.MemberDefinition.IsClass;
            this.Target.IsExplicitLayout = this.Source.MemberDefinition.IsExplicitLayout;
            this.Target.IsImport = this.Source.MemberDefinition.IsImport;
            this.Target.IsInterface = this.Source.MemberDefinition.IsInterface;
            this.Target.IsNestedAssembly = this.Source.MemberDefinition.IsNestedAssembly;
            this.Target.IsNestedFamily = this.Source.MemberDefinition.IsNestedFamily;
            this.Target.IsNestedFamilyAndAssembly = this.Source.MemberDefinition.IsNestedFamilyAndAssembly;
            this.Target.IsNestedFamilyOrAssembly = this.Source.MemberDefinition.IsNestedFamilyOrAssembly;
            this.Target.IsNestedPrivate = this.Source.MemberDefinition.IsNestedPrivate;
            this.Target.IsNestedPublic = this.Source.MemberDefinition.IsNestedPublic;
            this.Target.IsNotPublic = this.Source.MemberDefinition.IsNotPublic;
            this.Target.IsPublic = this.Source.MemberDefinition.IsPublic;
            this.Target.IsRuntimeSpecialName = this.Source.MemberDefinition.IsRuntimeSpecialName;
            this.Target.IsSealed = this.Source.MemberDefinition.IsSealed;
            this.Target.IsSequentialLayout = this.Source.MemberDefinition.IsSequentialLayout;
            this.Target.IsSerializable = this.Source.MemberDefinition.IsSerializable;
            this.Target.IsSpecialName = this.Source.MemberDefinition.IsSpecialName;
            this.Target.IsUnicodeClass = this.Source.MemberDefinition.IsUnicodeClass;
            this.Target.IsValueType = this.Source.MemberDefinition.IsValueType;
            this.Target.IsWindowsRuntime = this.Source.MemberDefinition.IsWindowsRuntime;
            this.Target.PackingSize = this.Source.MemberDefinition.PackingSize;
            this.Target.Scope = this.Source.MemberDefinition.Scope;

            this.Target.BaseType = this.Source.MemberDefinition.BaseType;
            this.Target.DeclaringType = this.Source.MemberDefinition.DeclaringType;

            this.Target.MetadataToken = this.Source.MemberDefinition.MetadataToken;

            //this.Target.CustomAttributes = this.Source.MemberDefinition;
            //this.Target.GenericParameters = this.Source.MemberDefinition;
            //this.Target.Interfaces = this.Source.MemberDefinition;
            //this.Target.SecurityDeclarations = this.Source.MemberDefinition;
        }

        private void PopulateMixersWithScaffolding()
        {
            if (!this.IsScaffoldingCompleted)
            {
                var voidReference = this.Target.Module.Import(typeof(void));

                foreach (var source in from type in this.Source.MemberInfo.GetNestedTypes()
                                       where !type.IsSkipped()
                                       select new TypeWithRespectToModule(type, this.Target.Module))
                {
                    var target = new TypeDefinition(
                        source.MemberDefinition.Namespace,
                        source.MemberDefinition.Name,
                        0);
                    target.Module.Types.Add(target);
                    this.Target.NestedTypes.Add(target);

                    var typeMixer = new TypeMixer(target, source);
                    typeMixer.PopulateMixersWithScaffolding();
                    this.Mixers.Add(typeMixer);
                }

                foreach (var source in from field in this.Source.MemberInfo.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                                       where !field.IsSkipped()
                                       select new FieldWithRespectToModule(field, this.Target.Module))
                {
                    var target = new FieldDefinition(source.MemberInfo.Name, 0, voidReference);
                    this.Target.Fields.Add(target);
                    this.Mixers.Add(new FieldMixer(target, source));
                }

                foreach (var source in from method in this.Source.MemberInfo.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                                       where !method.IsConstructor && !method.IsSkipped()
                                       select new MethodWithRespectToModule(method, this.Target.Module))
                {
                    var target = new MethodDefinition(source.MemberDefinition.Name, 0, voidReference);
                    this.Target.Methods.Add(target);
                    this.Mixers.Add(new MethodMixer(target, source));
                }

                foreach (var source in from property in this.Source.MemberInfo.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                                       where !property.IsSkipped()
                                       select new PropertyWithRespectToModule(property, this.Target.Module))
                {
                    var target = new PropertyDefinition(source.MemberDefinition.Name, 0, voidReference);
                    this.Target.Properties.Add(target);
                    this.Mixers.Add(new PropertyMixer(target, source));
                }

                foreach (var source in from @event in this.Source.MemberInfo.GetEvents(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                                       where !@event.IsSkipped()
                                       select new EventWithRespectToModule(@event, this.Target.Module))
                {
                    var target = new EventDefinition(source.MemberDefinition.Name, 0, voidReference);
                    this.Target.Events.Add(target);
                    this.Mixers.Add(new EventMixer(target, source));
                }
            }
            this.IsScaffoldingCompleted = true;
        }
    }
}
