using af = Autofac;
using Autofac;
using System;
using System.Reflection;

namespace Bix.Autofac
{
    public abstract class RepositoryAutofacModuleBase : af.Module
    {
        public RepositoryAutofacModuleBase(
            Func<Type, bool> typePredicate,
            AutofacModuleInstanceScope scope = AutofacModuleInstanceScope.SingleInstance)
        {
            this.TypePredicate = typePredicate;
            this.Scope = scope;
        }

        public Func<Type, bool> TypePredicate { get; }
        public AutofacModuleInstanceScope Scope { get; }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterAssemblyTypes(this.GetType().GetTypeInfo().Assembly)
                .Where(this.TypePredicate)
                .AsImplementedInterfaces()
                .AppendScope(this.Scope);
        }
    }
}
