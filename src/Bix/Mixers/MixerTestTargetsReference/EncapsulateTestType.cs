﻿/***************************************************************************/
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
using System.Runtime.Serialization;

namespace Bix.Mixers.MixerTestTargets
{
    public class EncapsulatesTestType : IEncapsulates
    {
        #region

        // empty constructor is in pre-mixed type by default
        public EncapsulatesTestType() { }

        #endregion

        #region Copied Code

        #region Encapsulated

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

        #endregion

        #region Not Encapsulated

        public int PublicGetPublicSetNotEncapsulated { get; set; }
        public int PublicGetPrivateSetNotEncapsulated { get; private set; }
        public int PublicGetInternalSetNotEncapsulated { get; internal set; }
        public int PublicGetProtectedSetNotEncapsulated { get; protected set; }
        public int PublicGetProtectedInternalSetNotEncapsulated { get; protected internal set; }

        public int PrivateGetPublicSetNotEncapsulated { private get; set; }
        private int PrivateGetPrivateSetNotEncapsulated { get; set; }
        internal int PrivateGetInternalSetNotEncapsulated { private get; set; }
        protected int PrivateGetProtectedSetNotEncapsulated { private get; set; }
        protected internal int PrivateGetProtectedInternalSetNotEncapsulated { private get; set; }

        public int InternalGetPublicSetNotEncapsulated { internal get; set; }
        internal int InternalGetPrivateSetNotEncapsulated { get; private set; }
        internal int InternalGetInternalSetNotEncapsulated { get; set; }
        protected internal int InternalGetProtectedInternalSetNotEncapsulated { internal get; set; }

        public int ProtectedGetPublicSetNotEncapsulated { protected get; set; }
        protected int ProtectedGetPrivateSetNotEncapsulated { get; private set; }
        protected int ProtectedGetProtectedSetNotEncapsulated { get; set; }
        protected internal int ProtectedGetProtectedInternalSetNotEncapsulated { protected get; set; }

        public int ProtectedInternalGetPublicSetNotEncapsulated { protected internal get; set; }
        protected internal int ProtectedInternalGetPrivateSetNotEncapsulated { get; private set; }
        protected internal int ProtectedInternalGetInternalSetNotEncapsulated { get; internal set; }
        protected internal int ProtectedInternalGetProtectedSetNotEncapsulated { get; protected set; }
        protected internal int ProtectedInternalGetProtectedInternalSetNotEncapsulated { get; set; }

        private int publicGetNotEncapsulated;
        public int PublicGetNotEncapsulated { get { return this.publicGetNotEncapsulated; } }

        private int privateGetNotEncapsulated;
        private int PrivateGetNotEncapsulated { get { return this.privateGetNotEncapsulated; } }

        private int internalGetNotEncapsulated;
        internal int InternalGetNotEncapsulated { get { return this.internalGetNotEncapsulated; } }

        private int protectedGetNotEncapsulated;
        protected int ProtectedGetNotEncapsulated { get { return this.protectedGetNotEncapsulated; } }

        private int protectedInternalGetNotEncapsulated;
        protected internal int ProtectedInternalGetNotEncapsulated { get { return this.protectedInternalGetNotEncapsulated; } }

        private int publicSetNotEncapsulated;
        public int PublicSetNotEncapsulated { set { this.publicSetNotEncapsulated = value; } }

        private int privateSetNotEncapsulated;
        private int PrivateSetNotEncapsulated { set { this.privateSetNotEncapsulated = value; } }

        private int internalSetNotEncapsulated;
        internal int InternalSetNotEncapsulated { set { this.internalSetNotEncapsulated = value; } }

        private int protectedSetNotEncapsulated;
        protected int ProtectedSetNotEncapsulated { set { this.protectedSetNotEncapsulated = value; } }

        private int protectedInternalSetNotEncapsulated;
        protected internal int ProtectedInternalSetNotEncapsulated { set { this.protectedInternalSetNotEncapsulated = value; } }

        #endregion

        #endregion

        #region Reference for DTO Generated by Encapsulation Mixer

        public class Dto
        {
            public int PublicGetPublicSet { get; set; }
            public int PublicGetPrivateSet { get; set; }
            public int PublicGetInternalSet { get; set; }
            public int PublicGetProtectedSet { get; set; }
            public int PublicGetProtectedInternalSet { get; set; }
            public int PrivateGetPublicSet { get; set; }
            public int PrivateGetPrivateSet { get; set; }
            public int PrivateGetInternalSet { get; set; }
            public int PrivateGetProtectedSet { get; set; }
            public int PrivateGetProtectedInternalSet { get; set; }
            public int InternalGetPublicSet { get; set; }
            public int InternalGetPrivateSet { get; set; }
            public int InternalGetInternalSet { get; set; }
            public int InternalGetProtectedInternalSet { get; set; }
            public int ProtectedGetPublicSet { get; set; }
            public int ProtectedGetPrivateSet { get; set; }
            public int ProtectedGetProtectedSet { get; set; }
            public int ProtectedGetProtectedInternalSet { get; set; }
            public int ProtectedInternalGetPublicSet { get; set; }
            public int ProtectedInternalGetPrivateSet { get; set; }
            public int ProtectedInternalGetInternalSet { get; set; }
            public int ProtectedInternalGetProtectedSet { get; set; }
            public int ProtectedInternalGetProtectedInternalSet { get; set; }
            public int PublicGet { get; set; }
            public int PrivateGet { get; set; }
            public int InternalGet { get; set; }
            public int ProtectedGet { get; set; }
            public int ProtectedInternalGet { get; set; }
            public int PublicSet { get; set; }
            public int PrivateSet { get; set; }
            public int InternalSet { get; set; }
            public int ProtectedSet { get; set; }
            public int ProtectedInternalSet { get; set; }
        }

        #endregion

        #region Reference for ISerializable Implementation

        private EncapsulatesTestType(SerializationInfo info, StreamingContext context)
        {
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
        }

        #endregion
    }
}