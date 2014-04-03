using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.CecilMixer.CommonMixing
{
    public class SerializableSource : ISerializable
    {
        private SerializableSource(SerializationInfo info, StreamingContext context)
        {
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
        }
    }
}
