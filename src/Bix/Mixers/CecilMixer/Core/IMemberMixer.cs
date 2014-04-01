using System;
using System.Diagnostics.Contracts;

namespace Bix.Mixers.CecilMixer.Core
{
    [ContractClass(typeof(IMemberMixerContract))]
    internal interface IMemberMixer
    {
        bool IsMixed { get; }

        void Mix();
    }
}
