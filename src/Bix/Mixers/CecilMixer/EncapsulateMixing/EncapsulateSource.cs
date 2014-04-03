using Bix.Mix.Encapsulate;
using Bix.Mixers.CecilMixer.Core;
using System;
using System.Runtime.CompilerServices;

namespace Bix.Mixers.CecilMixer.EncapsulateMixing
{
    internal class EncapsulateSource : IEncapsulates
    {
        [Skip]
        public EncapsulateSource() { }

        [CompilerGenerated]
        IEncapsulator IEncapsulates.Encapsulator { get; set; }

        #region Skipped Items

        [Skip]
        System.Collections.ObjectModel.ReadOnlyCollection<Mix.IMixer> Mix.IMixes.Mixers
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        [Skip]
        void System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
