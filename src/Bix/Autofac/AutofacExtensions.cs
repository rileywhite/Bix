using Autofac;
using Autofac.Builder;
using Hangfire;
using System;

namespace Bix.Autofac
{
    public static class AutofacExtensions
    {
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle>
            AppendScope<TLimit, TActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> source,
                AutofacModuleInstanceScope scope)
        {
            switch (scope)
            {
                case AutofacModuleInstanceScope.InstancePerLifetimeScope:
                    return source.InstancePerLifetimeScope();

                case AutofacModuleInstanceScope.InstancePerRequest:
                    return source.InstancePerRequest();

                case AutofacModuleInstanceScope.InstancePerBackgroundJob:
                    return source.InstancePerBackgroundJob();

                default:
                    return source.SingleInstance();
            }
        }
    }
}
