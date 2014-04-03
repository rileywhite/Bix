using Bix.Mix;
using Bix.Mixers.CecilMixer.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.CecilMixer.CommonMixing
{
    internal class MixesSource : IMixes
    {
        [Skip]
        public MixesSource() { }

        [CompilerGenerated]
        ReadOnlyCollection<IMixer> IMixes.Mixers { get; set; }

        [Skip]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotSupportedException();
        }
    }
}
