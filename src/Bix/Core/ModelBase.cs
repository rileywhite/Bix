using Newtonsoft.Json;
using System;

namespace Bix.Core
{
    public abstract class ModelBase
    {
        /// <summary>
        /// Gets whether this is an aggregate root, meaning that it is the root
        /// of a graph of models through which the graph can be created, saved, updated,
        /// or deleted.
        /// </summary>
        [JsonIgnore]
        public bool IsAggregateRoot
        {
            get => this.GetType().IsAggregateRootModelType();
        }
    }

    public abstract class ModelBase<TModel, TIdentity> : ModelBase, IHasIdentity<TIdentity>
    where TModel : ModelBase<TModel, TIdentity>, new()
    {
        public abstract TIdentity Identity { get; }
    }
}
