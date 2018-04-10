using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace NaiveIME
{
    public interface IFrequencyStatistics<K>
    {
        int Total { get; }
        int Count { get; }

        IEnumerable<KeyValuePair<K, int>> KeyFrequencyDescending { get; }
        IEnumerable<KeyValuePair<K, int>> KeyFrequencyAscending { get; }
        IEnumerable<KeyValuePair<K, int>> KeyFrequency { get; }

        int this[K key] { get; set; }
        void Add(K key, int count = 1);
        void Clear();
    }

    public static class FrequencyStatisticsExtension
    {
        public static void Add<TKey>(this IFrequencyStatistics<TKey> stat, IEnumerable<KeyValuePair<TKey, int>> pairs)
        {
            foreach (var pair in pairs)
                stat.Add(pair.Key, pair.Value);
        }

        public static void WriteToCsv<TKey>(this IFrequencyStatistics<TKey> stat, TextWriter writer, int take = -1)
        {
            foreach (var pair in take == -1 ? stat.KeyFrequency : stat.KeyFrequency.Take(take))
                writer.WriteLine($"{pair.Key}, {pair.Value}");
        }

        public static void ReadFromCsv<TKey>(this IFrequencyStatistics<TKey> stat, TextReader reader, Func<string, TKey> keyParser)
        {
            stat.Clear();
            while (reader.Peek() != -1)
            {
                var ss = reader.ReadLine().Split(',');
                var key = keyParser(ss[0]);
                var freq = int.Parse(ss[1]);
                stat.Add(key, freq);
            }
        }
    }

    [Serializable, JsonObject(MemberSerialization.OptIn)]
	public class FrequencyStatistics<T>: IFrequencyStatistics<T>
	{
		[JsonProperty("total")]
		public int Total { get; private set; }

		[JsonProperty("dict")]
		readonly IDictionary<T, int> frequencyDict = new Dictionary<T, int>();

		public int Count => frequencyDict.Count;

		public IEnumerable<KeyValuePair<T, int>> KeyFrequencyDescending =>
			from pair in frequencyDict orderby pair.Value descending select pair;

		public IEnumerable<KeyValuePair<T, int>> KeyFrequencyAscending =>
			from pair in frequencyDict orderby pair.Value select pair;

		public IEnumerable<KeyValuePair<T, int>> KeyFrequency => frequencyDict;

		public int this[T key]
		{
			get
			{
				return frequencyDict.GetOrDefault(key);
			}

			set
			{
				if (frequencyDict.ContainsKey(key))
					frequencyDict[key] += value;
				else
					frequencyDict.Add(key, value);
				Total += value;
			}
		}

		public void Add(T c, int count = 1)
		{
			if (frequencyDict.ContainsKey(c))
				frequencyDict[c] += count;
			else
				frequencyDict.Add(c, count);
			Total += count;
		}

		public FrequencyStatistics()
		{ 
		}

		public FrequencyStatistics(IEnumerable<KeyValuePair<T, int>> dict)
		{
			foreach (var pair in dict)
				this.frequencyDict.Add(pair.Key, pair.Value);
		}

		public FrequencyStatistics<T> Where(Func<T, bool> predictFunc)
		{
			return new FrequencyStatistics<T>(frequencyDict.Where(pair => predictFunc(pair.Key)));
		}

		public FrequencyStatistics<T> Where(Func<KeyValuePair<T, int>, bool> predictFunc)
		{
			return new FrequencyStatistics<T>(frequencyDict.Where(pair => predictFunc(pair)));
		}

		public FrequencyStatistics<T1> Select<T1>(Func<T, T1> func)
		{
			return new FrequencyStatistics<T1>(frequencyDict.Select(
				pair => new KeyValuePair<T1, int>(func(pair.Key), pair.Value)));
		}

		public FrequencyStatistics<T> Take(int count)
		{
			return new FrequencyStatistics<T>(KeyFrequencyDescending.Take(count));
		}

		public Distribution<T> ToDistribution()
		{
			return new Distribution<T>(frequencyDict.Select(pair => 
			       		new KeyValuePair<T, float>(pair.Key, (float)pair.Value / Total)));
		}

		public void Clear()
		{
			frequencyDict.Clear();
			Total = 0;
		}
	}
}
