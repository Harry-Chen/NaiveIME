using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace NaiveIME
{
	public class PinyinConverter
	{
		IDictionary<char, List<string>> _charToPinyins = new Dictionary<char, List<string>>();
		IDictionary<string, SortedSet<char>> _pinyinToChars = new Dictionary<string, SortedSet<char>>();
		ICollection<string> Pinyins => _pinyinToChars.Keys;

		public ICollection<string> ConvertCharToPinyin(char c)
		{
			return _charToPinyins.GetOrDefault(c);
		}

		public IEnumerable<string> GetPinyinsStartsWith(string str)
		{
			return Pinyins.Where(py => py.StartsWith(str));
		}

		public ISet<char> ConvertPinyinToChar(string pinyin)
		{
			return _pinyinToChars.GetOrDefault(pinyin);
		}

		public void Add(char c, string pinyin)
		{
			_pinyinToChars.GetOrAddDefault(pinyin).Add(c);
			_charToPinyins.GetOrAddDefault(c).Add(pinyin);
		}

		private void LoadFromFile(string filePath)
		{
			foreach (var line in File.ReadLines(filePath))
			{
				var tokens = line.Split();
				var pinyin = tokens[0];
				foreach (var s in tokens.Skip(1))
					Add(s[0], pinyin);
			}
		}

		public PinyinConverter() { }
		public PinyinConverter(string filePath)
		{
			LoadFromFile(filePath);
		}

		private static Lazy<PinyinConverter> lazyInstance = new Lazy<PinyinConverter>(() =>
		    new PinyinConverter(PersistentConfiguration.DefaultPinyinFile));
		public static PinyinConverter INSTANCE = lazyInstance.Value;
	}
}