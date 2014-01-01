using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class FieldMixer : MemberMixerBase<FieldDefinition, FieldWithRespectToModule>
    {
        public FieldMixer(FieldDefinition target, FieldWithRespectToModule source)
            : base(target, source) { }

        public override void Mix()
        {
            throw new NotImplementedException();
        }
    }
}
