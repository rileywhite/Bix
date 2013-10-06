using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bix.Mix.Encapsulate;

namespace Bix.Pix.ChiaTests.Utility
{
    [Encapsulates]
    internal class EncapsulatesTestType
    {
        [Encapsulated]
        public int PublicGetPublicSet { get; set; }
        [Encapsulated]
        public int PublicGetPrivateSet { get; private set; }
        [Encapsulated]
        public int PublicGetInternalSet { get; internal set; }
        [Encapsulated]
        public int PublicGetProtectedSet { get; protected set; }
        [Encapsulated]
        public int PublicGetProtectedInternalSet { get; protected internal set; }

        [Encapsulated]
        public int PrivateGetPublicSet { private get; set; }
        [Encapsulated]
        private int PrivateGetPrivateSet { get; set; }
        [Encapsulated]
        internal int PrivateGetInternalSet { private get; set; }
        [Encapsulated]
        protected int PrivateGetProtectedSet { private get; set; }
        [Encapsulated]
        protected internal int PrivateGetProtectedInternalSet { private get; set; }

        [Encapsulated]
        public int InternalGetPublicSet { internal get; set; }
        [Encapsulated]
        internal int InternalGetPrivateSet { get; private set; }
        [Encapsulated]
        internal int InternalGetInternalSet { get; set; }
        [Encapsulated]
        protected internal int InternalGetProtectedInternalSet { internal get; set; }

        [Encapsulated]
        public int ProtectedGetPublicSet { protected get; set; }
        [Encapsulated]
        protected int ProtectedGetPrivateSet { get; private set; }
        [Encapsulated]
        protected int ProtectedGetProtectedSet { get; set; }
        [Encapsulated]
        protected internal int ProtectedGetProtectedInternalSet { protected get; set; }

        [Encapsulated]
        public int ProtectedInternalGetPublicSet { protected internal get; set; }
        [Encapsulated]
        protected internal int ProtectedInternalGetPrivateSet { get; private set; }
        [Encapsulated]
        protected internal int ProtectedInternalGetInternalSet { get; internal set; }
        [Encapsulated]
        protected internal int ProtectedInternalGetProtectedSet { get; protected set; }
        [Encapsulated]
        protected internal int ProtectedInternalGetProtectedInternalSet { get; set; }
    }
}
