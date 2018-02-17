﻿/***************************************************************************/
// Copyright 2013-2018 Riley White
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

using System;

namespace Bix.Core
{
    public abstract class ViewModelPropertyTracker
    {
        public static ViewModelPropertyTracker<T> Create<T>(Func<T> oldValueReader) => new ViewModelPropertyTracker<T>(oldValueReader);

        public abstract bool IsDirty { get; }
    }

    public class ViewModelPropertyTracker<T> : ViewModelPropertyTracker
    {
        internal ViewModelPropertyTracker(Func<T> oldValueReader)
        {
            this.OldValueReader = oldValueReader;
        }

        public Func<T> OldValueReader { get; }

        private bool NewValueIsSet { get; set; }

        private T newValue;
        public T Value
        {
            get => this.NewValueIsSet ? this.newValue : this.OldValueReader();
            set
            {
                this.NewValueIsSet = true;
                this.newValue = value;
            }
        }

        public override bool IsDirty
        {
            get
            {
                if (!this.NewValueIsSet) { return false; }

                var oldValue = this.OldValueReader();
                var newValue = this.newValue;

                if ((oldValue == null) != (newValue == null)) { return true; }
                return oldValue == null ? false : !oldValue.Equals(newValue);
            }
        }
    }
}
