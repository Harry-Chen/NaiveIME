using Newtonsoft.Json;
using System;

namespace NaiveIME
{
    [Serializable, JsonObject(MemberSerialization.OptIn)]
	public abstract class NGramBase
	{
		// Shared Property
	    public string SourceName { get; set; }

	    // Abstract Method
	    public virtual void FromAnalyzer(TextAnalyzer analyzer)
	    {
	        SourceName = analyzer.SourceName;
	    }
		public abstract Distribution<string> GetDistribution(string pre);
		public virtual void Compress() 
		{
			throw new NotImplementedException();
		}

		public virtual Distribution<string> GetDistribution(PinyinToSolve condition)
		{
			var dtb = GetDistribution(condition.Chars);
			if(condition.Pinyin != null)
				dtb = dtb.Where(str => str.Substring(1).Equals(condition.Pinyin));
			return dtb;
		}
	}
}
