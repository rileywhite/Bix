using System;
using System.Collections.Generic;

namespace Bix.Core
{
    public class InMemoryCache : ICache
    {
        private Dictionary<string, object> Items { get; } = new Dictionary<string, object>();

        public T Get<T>(string key)
        {
            if (key == null) { throw new ArgumentNullException(nameof(key)); }
            return (T)this.Items[key];
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (key == null) { throw new ArgumentNullException(nameof(key)); }
            if (!this.Items.TryGetValue(key, out object valueAsObject))
            {
                value = default(T);
                return false;
            }

            value = (T)valueAsObject;
            return true;
        }

        public object Get(string key)
        {
            if (key == null) { throw new ArgumentNullException(nameof(key)); }
            return this.Items[key];
        }

        public bool TryGet(string key, out object value)
        {
            if (key == null) { throw new ArgumentNullException(nameof(key)); }
            return this.Items.TryGetValue(key, out value);
        }

        public void Set<T>(string key, T value)
        {
            if (key == null) { throw new ArgumentNullException(nameof(key)); }
            this.Items[key] = value;
        }
    }
}
