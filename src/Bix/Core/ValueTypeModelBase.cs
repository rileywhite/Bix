using System;

namespace Bix.Core
{
    public abstract class ValueTypeModelBase<TModel, TIdentity, TNaturalKey>
        : ModelBase<TModel, TIdentity>, IHasNaturalKey<TNaturalKey>
        where TModel : ValueTypeModelBase<TModel, TIdentity, TNaturalKey>, new()
    {
        public abstract TNaturalKey NaturalKey { get; }
    }

    public abstract class ValueTypeModelBase<TModel, TNaturalKey>
        : ValueTypeModelBase<TModel, TNaturalKey, TNaturalKey>
        where TModel : ValueTypeModelBase<TModel, TNaturalKey>, new()
    {
        public override sealed TNaturalKey Identity => this.NaturalKey;
    }
}
