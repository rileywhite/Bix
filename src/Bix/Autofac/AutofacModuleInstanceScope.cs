using System;

namespace Bix.Autofac
{
    public enum AutofacModuleInstanceScope
    {
        /// <summary>
        /// Potentially a good fit for desktop apps
        /// </summary>
        SingleInstance,

        /// <summary>
        /// Potentially a good fit for mvc/web api apps
        /// </summary>
        InstancePerLifetimeScope,

        /// <summary>
        /// Potentially a good fit for web forms apps
        /// </summary>
        InstancePerRequest,

        /// <summary>
        /// Provided by Hangfire.Autofac for use in Hangfire queued background jobs.
        /// Used for a single instance per background task.
        /// </summary>
        InstancePerBackgroundJob,
    }
}
