/***************************************************************************/
// EncapsulatesTestType.cs
// Copyright 2013 Riley White
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
/***************************************************************************/

using Bix.Mix.Encapsulate;
using System;

namespace Bix.Picks.MixTestTargets
{
    public class EncapsulatesTestType
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

        private int publicGet;
        [Encapsulated]
        public int PublicGet { get { return this.publicGet; } }

        private int privateGet;
        [Encapsulated]
        private int PrivateGet { get { return this.privateGet; } }

        private int internalGet;
        [Encapsulated]
        internal int InternalGet { get { return this.internalGet; } }

        private int protectedGet;
        [Encapsulated]
        protected int ProtectedGet { get { return this.protectedGet; } }

        private int protectedInternalGet;
        [Encapsulated]
        protected internal int ProtectedInternalGet { get { return this.protectedInternalGet; } }

        private int publicSet;
        [Encapsulated]
        public int PublicSet { set { this.publicSet = value; } }

        private int privateSet;
        [Encapsulated]
        private int PrivateSet { set { this.privateSet = value; } }

        private int internalSet;
        [Encapsulated]
        internal int InternalSet { set { this.internalSet = value; } }

        private int protectedSet;
        [Encapsulated]
        protected int ProtectedSet { set { this.protectedSet = value; } }

        private int protectedInternalSet;
        [Encapsulated]
        protected internal int ProtectedInternalSet { set { this.protectedInternalSet = value; } }
    }
}
