using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class EventMixer : MemberMixerBase<EventDefinition, EventWithRespectToModule>
    {
        public EventMixer(EventDefinition target, EventWithRespectToModule source)
            : base(target, source) { }

        public override void Mix()
        {
            throw new NotImplementedException();
        }
    }
}
