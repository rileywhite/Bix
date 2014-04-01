using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class EventWithRespectToModule
        : MemberWithRespectToModuleBase<EventInfo, EventDefinition>
    {
        public EventWithRespectToModule(EventInfo @event, ModuleDefinition referencingModule)
            : base(@event, referencingModule) { }

        public EventImporter memberImporter = new EventImporter();
        public override IMemberImporter<EventInfo, EventDefinition> MemberImporter
        {
            get { return this.memberImporter; }
        }
    }
}
