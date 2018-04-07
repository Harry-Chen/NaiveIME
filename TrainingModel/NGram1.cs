using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;

namespace NaiveIME
{
	[Serializable]
	public class NGram1: NGramBase
	{
		[JsonProperty]
		Distribution<string> dtb = new Distribution<string>();

		public override Distribution<string> GetDistribution(string pre)
		{
			return dtb;
		}
		public override void FromAnalyzer(TextAnalyzer stat)
		{
		    base.FromAnalyzer(stat);
			var stat0 = new FrequencyStatistics<string>();
			foreach (var pair in stat.StringFrequency)
			{
                var source = pair.Key.Split(':');
                if (source.Length != 2) continue;
                var word = source[0];
                var pinyin = source[1];
                
                if (word.Length != 1)
                    continue;
                char c = word[0];
                int freq = pair.Value;
                stat0.Add(c == '$' ? "$" : c + pinyin, freq);
			}
			dtb = stat0.ToDistribution();
		}
	}
}
