using Mono.Cecil;
using System;
using System.Reflection;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class EventWithRespectToModule
        : MemberWithRespectToModule<EventInfo, EventReference, EventDefinition>
    {
        public EventWithRespectToModule(EventInfo @event, ModuleDefinition referencingModule)
            : base(@event, referencingModule) { }
    }
}
