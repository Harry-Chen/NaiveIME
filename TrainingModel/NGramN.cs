using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NaiveIME
{
    using MixStrategy = Func<IEnumerable<Distribution<string>>, Distribution<string>>;
    /// <summary>
    /// 用字典实现的通用N-Gram模型
    /// </summary>
    public class NGramN: NGramBase
	{
		[JsonProperty]
		Dictionary<string, Distribution<string>> dict = new Dictionary<string, Distribution<string>>();

	    public MixStrategy MixDistributeStrategy = NGramMixed.MixStrategyCoefficient;

		public override Distribution<string> GetDistribution(PinyinToSolve condition)
		{
			var list = new List<Distribution<string>>();
			for (int n = 0; n < 6; ++n)
			{
				var dtb = dict.GetOrDefault(condition.Chars.LastSubString(n, '^'));
				if (condition.Pinyin != null)
					dtb = dtb.Where(str => str.Substring(1).Equals(condition.Pinyin));

				list.Add(dtb);
			}
			//Console.WriteLine($"{condition} {list.Count-1}");
		    return MixDistributeStrategy(list);
		}
		public override Distribution<string> GetDistribution(string pre)
		{
			throw new NotImplementedException();
		}
		public override void FromAnalyzer(TextAnalyzer stat)
		{
		    base.FromAnalyzer(stat);
			var statByPre = new Dictionary<string, FrequencyStatistics<string>>();
			foreach (var pair in stat.StringFrequency)
			{
                var source = pair.Key.Split(':');
                if (source.Length != 2) continue;
                var word = source[0];
                var pinyin = source[1];
                string pre = word.Substring(0, word.Length - 1);
                char c = word[word.Length - 1];
                int freq = pair.Value;
                statByPre.GetOrAddDefault(pre).Add(c == '$' ? "$" : c + pinyin, freq);
			}
			dict = statByPre.ToDictionary(pair => pair.Key, pair => pair.Value.ToDistribution());
		}
	}
}
