/***************************************************************************/
// Copyright 2013-2022 Riley White
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
    /// <summary>
    /// Base type for property trackers used in view-models to track original and updated values for dirty-detection
    /// and reverting behavior.
    /// </summary>
    /// <remarks>
    /// This type is generally used internally when the specific type of the property doesn't matter.
    /// Use <see cref="ViewModelPropertyTracker.Create{T}(Func{T})"/> to create a new tracker.
    /// </remarks>
    public abstract class ViewModelPropertyTracker
    {
        /// <summary>
        /// Creates a new tracker
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="oldValueReader">Function that returns the original value when executed</param>
        /// <returns></returns>
        public static ViewModelPropertyTracker<T> Create<T>(Func<T> oldValueReader) => new ViewModelPropertyTracker<T>(oldValueReader);

        /// <summary>
        /// <c>true</c> if the current value and original value differ, else <c>false</c>.
        /// </summary>
        public abstract bool IsDirty { get; }
    }

    /// <summary>
    /// Base type for property trackers used in view-models to track original and updated values for dirty-detection
    /// </summary>
    /// <typeparam name="T">Type of the property</typeparam>
    public class ViewModelPropertyTracker<T> : ViewModelPropertyTracker
    {
        /// <summary>
        /// Creates a new <see cref="ViewModelPropertyTracker{T}"/>
        /// </summary>
        /// <param name="oldValueReader">Function that returns the original value when executed</param>
        internal ViewModelPropertyTracker(Func<T> oldValueReader)
        {
            this.OldValueReader = oldValueReader;
        }

        /// <summary>
        /// Gets the function that returns the original value when executed
        /// </summary>
        public Func<T> OldValueReader { get; }

        private bool NewValueIsSet { get; set; }

        private T newValue;

        /// <summary>
        /// Gets the current value of the property.
        /// </summary>
        public T Value
        {
            get => this.NewValueIsSet ? this.newValue : this.OldValueReader();
            set
            {
                this.NewValueIsSet = true;
                this.newValue = value;
            }
        }

        /// <summary>
        /// Gets whether this property's current value differes from its original value.
        /// </summary>
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
