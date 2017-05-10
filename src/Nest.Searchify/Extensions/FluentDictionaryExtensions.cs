using System;

namespace Nest.Searchify.Extensions
{
    internal static class FluentDictionaryExtensions
    {
        internal static FluentDictionary<TKey, TValue> AddOrUpdate<TKey, TValue>(this FluentDictionary<TKey, TValue> dictionary, TKey key, TValue value, Predicate<TValue> predicate = null)
        {
            if (predicate?.Invoke(value) ?? true)
            {
                if (dictionary.ContainsKey(key))
                {
                    dictionary[key] = value;
                }
                dictionary.Add(key, value);
            }
            return dictionary;
        }
    }
}