using Mono.Cecil;
using System;
using System.Reflection;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class EventImporter : MemberImporterBase<EventInfo, EventDefinition>
    {
        public override EventDefinition GetMemberDefinition(EventInfo memberInfo)
        {
            throw new NotImplementedException("Implement this when needed");
        }
    }
}
