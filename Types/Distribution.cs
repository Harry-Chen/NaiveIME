using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NaiveIME
{
    [Serializable, JsonObject(MemberSerialization.OptIn)]
	public class Distribution<T>
	{
		[JsonProperty("dict")]
		IDictionary<T, float> dict = new Dictionary<T, float>();

		public IEnumerable<KeyValuePair<T, float>> KeyProbDescending =>
			from pair in dict orderby pair.Value descending select pair;

		public IEnumerable<KeyValuePair<T, float>> KeyProbAscending =>
			from pair in dict orderby pair.Value select pair;

		public bool IsEmpty => dict.Count == 0;

		public float GetProbability(T key)
		{
			return dict.GetOrDefault(key);
		}

		public Distribution()
		{
			
		}

		public Distribution(IEnumerable<KeyValuePair<T, float>> dict)
		{
			foreach (var pair in dict)
				this.dict.Add(pair.Key, pair.Value);
//			Normalize();
		}

		static public Distribution<T> Empty()
		{
			return new Distribution<T>();
		}

		static public Distribution<T> Single(T key)
		{
			var obj = new Distribution<T>();
			obj.dict.Add(key, 1);
			return obj;
		}

		static public Distribution<T> Evenly(params T[] keys)
		{
			return new Distribution<T>(keys.Select(key => new KeyValuePair<T, float>(key, 1f / keys.Length)));
		}

		void Normalize()
		{
			float sum = dict.Values.Sum();
			if (Math.Abs(sum - 1) < 1e-5)
				return;
			dict = dict.ToDictionary(pair => pair.Key, pair => pair.Value / sum);
		}

	    public Distribution<T> Norm()
	    {
            var dtb = new Distribution<T>(dict);
	        dtb.Normalize();
	        return dtb;
	    }

	    public Distribution<T> Where(Func<T, bool> prediction)
		{
			return new Distribution<T>(dict.Where(pair => prediction(pair.Key)));
		}

		public Distribution<T1> Select<T1>(Func<T, T1> func)
		{
			return new Distribution<T1>(dict.Select(
				pair => new KeyValuePair<T1, float>(func(pair.Key), pair.Value)));
		}

		public Distribution<T> Take(int count)
		{
			return new Distribution<T>(KeyProbDescending.Take(count));
		}

		public Distribution<T1> ExpandAndMerge<T1>(Func<T, Distribution<T1>> func)
		{
			var newPairs = from pair in dict
							from ppair in func(pair.Key).dict
			                group ppair.Value * pair.Value by ppair.Key into g
							select new KeyValuePair<T1, float>(g.Key, g.Sum());
			return new Distribution<T1>(newPairs);
		}

		public Distribution<T> ExpandAndMerge(Func<T, Distribution<T>> func, bool norm = true)
			=> ExpandAndMerge<T>(func);

		public void Print()
		{
			int count = 10;
			foreach (var pair in KeyProbDescending)
			{
				Console.WriteLine($"{pair.Key}({pair.Value})");
				if (count-- == 0) break;
			}
			Console.WriteLine();
		}
	}
}
