using System;
using Newtonsoft.Json;

namespace NaiveIME
{
	[JsonObject(MemberSerialization.OptIn)]
	public struct PinyinToSolve
	{
		public int N => Chars.Length;
		[JsonProperty]
		public string Chars { get; }
		[JsonProperty]
		public string Pinyin { get; }
		public override string ToString() => Chars;
		public PinyinToSolve(string str, string pinyin = "")
		{
			Chars = str;
			Pinyin = pinyin;
		}
		public PinyinToSolve Reduce()
		{
			if (N == 0)
				throw new InvalidOperationException();
			return new PinyinToSolve(Chars.Substring(1));
		}
	}
}
