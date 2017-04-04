﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace OgameBot.Utilities
{
    public static class DictionaryExtensions
    {
        public static TVal AddOrUpdate<TKey, TVal>(this Dictionary<TKey, TVal> dict, TKey key, Func<TVal> addFunc, Func<TKey, TVal, TVal> updateFunc)
        {
            TVal val;
            if (dict.TryGetValue(key, out val))
                return dict[key] = updateFunc(key, val);

            return dict[key] = addFunc();
        }

        public static TVal GetOrAdd<TKey, TVal>(this Dictionary<TKey, TVal> dict, TKey key, Func<TVal> addFunc)
        {
            TVal val;
            if (dict.TryGetValue(key, out val))
                return val;

            val = addFunc();
            dict.Add(key, val);
            return val;
        }

        public static bool TryRemove<TKey, TVal>(this Dictionary<TKey, TVal> dict, TKey key, out TVal value)
        {
            if (dict.TryGetValue(key, out value))
            {
                dict.Remove(key);
                return true;
            }

            return false;
        }

        public static void Merge<TKey, TVal>(this Dictionary<TKey, TVal> dict, Dictionary<TKey, TVal> source)
        {
            source.ToList().ForEach(x => dict[x.Key] = x.Value);
        }
    }
}