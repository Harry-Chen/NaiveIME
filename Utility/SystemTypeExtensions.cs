using System;
using System.Collections.Generic;

namespace NaiveIME
{
	public static class StringExtension
	{
		public static string LastSubString(this string str, int lastn, char space = ' ')
		{
			int l = str.Length;
			if (l >= lastn)
				return str.Substring(l - lastn);
			else
				return new string(space, lastn - l) + str;
		}
	}

    public static class DictionaryExtension
    {
        public static V GetOrAddDefault<K, V>(this IDictionary<K, V> dict, K key)
            where V : new()
        {
            if (!dict.TryGetValue(key, out V val))
            {
                val = new V();
                dict.Add(key, val);
            }
            return val;
        }

        public static V GetOrDefault<K, V>(this IDictionary<K, V> dict, K key)
            where V : new()
        {
            if (!dict.TryGetValue(key, out V val))
                val = new V();
            return val;
        }
    }
}
