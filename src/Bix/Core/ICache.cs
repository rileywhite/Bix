using System;

namespace Bix.Core
{
    public interface ICache
    {
        void Set<T>(string key, T value);
        T Get<T>(string key);
        bool TryGet<T>(string key, out T value);
        object Get(string key);
        bool TryGet(string key, out object value);
    }
}
