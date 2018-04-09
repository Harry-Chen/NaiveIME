using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NaiveIME
{
	public class NGramMixed : NGramBase
	{
        NGramBase[] Models { get; }
		public Func<IEnumerable<Distribution<string>>, Distribution<string>> MixDistributeStrategy { get; set; }
			= MixStrategyMaxN;

		public override PinyinConverter PinyinDict
		{
			set
			{
				base.PinyinDict = value;
				foreach(var ng in Models)
					ng.PinyinDict = value;
			}
		}

		public NGramMixed(params NGramBase[] models)
		{
			Models = models;
		}

		public override Distribution<string> GetDistribution(PinyinToSolve condition)
		{
		    var dtbs = Models.Take(Math.Min(condition.N, 2) + 1)
		                    .Select(m => m.GetDistribution(condition));
			return MixDistributeStrategy(dtbs);
		}

		public static Distribution<string> MixStrategyMaxN(IEnumerable<Distribution<string>> dtbs)
		{
			return dtbs.Last(dtb => !dtb.IsEmpty);
		}

		public static Distribution<string> MixStrategyCoefficient(IEnumerable<Distribution<string>> dtbs)
		{
			var dict = new Dictionary<Distribution<string>, float>();
			float rest = 1, lambda = 0.75f;
			foreach (var dtb in dtbs.Reverse())
			{
				dict.Add(dtb, rest * lambda);
				rest *= 1 - lambda;
			}
			return new Distribution<Distribution<string>>(dict).ExpandAndMerge(dtb => dtb);
		}

		public override void FromAnalyzer(TextAnalyzer stat)
		{
			throw new InvalidOperationException();
		}

		public override Distribution<string> GetDistribution(string pre)
		{
			throw new NotImplementedException();
		}
	}
}
