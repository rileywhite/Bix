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
