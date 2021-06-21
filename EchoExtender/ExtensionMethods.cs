using System;
using System.Collections.Generic;

namespace EchoExtender {
    public static class ExtensionMethods {


        public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value) {
            try {
                dict.Add(key, value);
                return true;
            }
            catch (Exception) {
                return false;
            }
        }
    }
}