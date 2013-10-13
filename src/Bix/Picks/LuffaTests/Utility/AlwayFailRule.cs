using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bix.Mix.Validate;

namespace Bix.Picks.LuffaTests.Utility
{
    public class ValidationAlwaysFails : IValidates
    {
        public ValidationAlwaysFails(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
        }
        void System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
        }
    }
}
