using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class EventMixer : MemberMixerBase<EventInfo, EventDefinition, EventWithRespectToModule>
    {
        public EventMixer(EventDefinition target, EventWithRespectToModule source)
            : base(target, source)
        {
            Contract.Requires(target != null);
            Contract.Requires(source != null);
        }

        public override void Mix()
        {
            // TODO event mixing
            throw new NotImplementedException("Implement event mixing when needed");
        }
    }
}
