using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class PropertyMixer : MemberMixerBase<PropertyDefinition, PropertyWithRespectToModule>
    {
        public PropertyMixer(PropertyDefinition target, PropertyWithRespectToModule source)
            : base(target, source) { }

        public override void Mix()
        {
            throw new NotImplementedException();
        }
    }
}
