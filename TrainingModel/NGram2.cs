using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace NaiveIME
{
    public class NGram2: NGramBase
	{
		[JsonProperty]
		Dictionary<char, Distribution<string>> dict = new Dictionary<char, Distribution<string>>();
		// char - distribute

		public override Distribution<string> GetDistribution(string pre)
		{
			return dict.GetOrDefault(pre.LastSubString(1, '^')[0]);
		}
		public override void FromAnalyzer(TextAnalyzer stat)
		{
		    base.FromAnalyzer(stat);
			var statByChar = new Dictionary<char, FrequencyStatistics<string>>();
			foreach (var pair in stat.StringFrequency)
			{
                var source = pair.Key.Split(':');
                if (source.Length != 2) continue;
                var word = source[0];
                var pinyin = source[1];
                if (word.Length != 2)
                    continue;
                char c0 = word[0];
                char c1 = word[1];
                int freq = pair.Value;
                statByChar.GetOrAddDefault(c0).Add(c1 == '$' ? "$" : c1 + pinyin, freq);
            }
			dict = statByChar.ToDictionary(pair => pair.Key, pair => pair.Value.ToDistribution());
		}
	}
}
