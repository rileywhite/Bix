using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class MethodMixer : MemberMixerBase<MethodDefinition, MethodWithRespectToModule>
    {
        public MethodMixer(MethodDefinition target, MethodWithRespectToModule source)
            : base(target, source) { }

        public override void Mix()
        {
            throw new NotImplementedException();
        }
    }
}
