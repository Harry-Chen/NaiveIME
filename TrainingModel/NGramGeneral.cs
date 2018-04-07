using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NaiveIME
{
	/// <summary>
	/// 用字典实现的通用N-Gram模型
	/// </summary>
	public class NGramGeneral: NGramBase
	{
		[JsonProperty]
		public int N { get; }
		[JsonProperty]
		Dictionary<string, Distribution<string>> dict = new Dictionary<string, Distribution<string>>();

		public NGramGeneral(int n)
		{
			if (n <= 0)
				throw new ArgumentException();
			N = n;
		}

		public override Distribution<string> GetDistribution(string pre)
		{
			return dict.GetOrDefault(pre.LastSubString(N - 1, '^'));
		}
		public override void FromAnalyzer(TextAnalyzer analyzer)
		{
		    base.FromAnalyzer(analyzer);
			var statByChar = new Dictionary<string, FrequencyStatistics<string>>();
			foreach (var pair in analyzer.StringFrequency)
			{
                var source = pair.Key.Split(':');
                if (source.Length != 2) continue;
                var word = source[0];
                var pinyin = source[1];
				if (word.Length != N)
					continue;
				string pre = word.Substring(0, N-1);
				char c2 = pair.Key[N-1];
				int freq = pair.Value;
				statByChar.GetOrAddDefault(pre).Add(c2 == '$' ? "$" : c2 + pinyin, freq);
			}
			dict = statByChar.ToDictionary(pair => pair.Key, pair => pair.Value.ToDistribution());
		}
	}
}
