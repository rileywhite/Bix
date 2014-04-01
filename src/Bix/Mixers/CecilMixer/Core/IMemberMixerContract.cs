using System;
using System.Diagnostics.Contracts;

namespace Bix.Mixers.CecilMixer.Core
{
    [ContractClassFor(typeof(IMemberMixer))]
    internal abstract class IMemberMixerContract : IMemberMixer
    {
        public bool IsMixed { get; private set; }

        public void Mix()
        {
            Contract.Requires(!this.IsMixed);
            Contract.Ensures(this.IsMixed);
        }
    }
}
