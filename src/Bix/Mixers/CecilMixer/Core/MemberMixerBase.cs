using System;
using System.Diagnostics.Contracts;

namespace Bix.Mixers.CecilMixer.Core
{
    internal abstract class MemberMixerBase<TMemberDefinition, TMemberWithRespectToModule>
    {
        public MemberMixerBase(TMemberDefinition target, TMemberWithRespectToModule source)
        {
            Contract.Requires(target != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.Target != null);
            Contract.Ensures(this.Source != null);

            this.Target = target;
            this.Source = source;
        }

        protected TMemberDefinition Target { get; private set; }

        protected TMemberWithRespectToModule Source { get; private set; }

        public bool IsMixed { get; private set; }

        public abstract void Mix();
    }
}
