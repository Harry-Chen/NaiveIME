using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace NaiveIME
{
    /// <summary>
    /// 文本频率统计器
    /// </summary>
    public class TextAnalyzer
    {
        public string SourceName { get; private set; }
        FrequencyStatistics<string> Statistics { get; set; } = new FrequencyStatistics<string>();
        public float MinRate { get; set; } = 1e-6f;

        public int Total => Statistics["*"];

        public TextAnalyzer()
        {
        }

        public TextAnalyzer(string filePath)
        {
            Load(filePath);
        }

        IEnumerable<PinyinToSolve> SubStringSelector(string sentence, string[] pinyins)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < sentence.Length; ++i)
            {
                builder.Clear();
                builder.Append(sentence[i]);
                yield return new PinyinToSolve(builder.ToString(), (i == 0 || i == sentence.Length - 1) ? "*" : pinyins[i - 1]);
                for (int j = i + 1; j < Math.Min(sentence.Length, i + 3); ++j)
                {
                    builder.Append(sentence[j]);
                    yield return new PinyinToSolve(builder.ToString(), j == sentence.Length - 1 ? "*" : pinyins[j - 1]);
                }
            }
        }

        IEnumerable<string> CharPairSelector(string str, int maxK = 2)
        {
            for (int k = 1; k <= maxK; ++k)
                for (int i = 0; i <= str.Length - k; ++i)
                    yield return str.Substring(i, k);
        }

        public bool InCharSet(char c)
        {
            const char CHINESE_CHAR_MIN = (char)0x4e00;
            const char CHINESE_CHAR_MAX = (char)0x9fbb;
            return c >= CHINESE_CHAR_MIN && c <= CHINESE_CHAR_MAX;
        }

        public void Analyze(TextReader reader)
        {
            var builder = new StringBuilder("^");
            while (reader.Peek() != -1)
            {
                var pinyins = reader.ReadLine().Split(' ');
                var sentence = reader.ReadLine();
                if (sentence.Length != pinyins.Length)
                {
                    throw new FormatException("拼音字数与句子长度不符合！句子：" + sentence + "，拼音：" + pinyins);
                }
                builder.Append(sentence).Append('$');
                Statistics.Add("*", builder.Length);
                foreach (var strAndPy in SubStringSelector(builder.ToString(), pinyins))
                    Statistics.Add(strAndPy.Chars + ':' + strAndPy.Pinyin);
                builder.Clear();
                builder.Append('^');

            }
        }
        public void Analyze(string str)
        {
            Analyze(new StringReader(str));
        }
        public void AnalyzeFile(string filePath)
        {
            using (var file = File.OpenText(filePath))
                Analyze(file);
        }
        public void AnalyzeFiles(IEnumerable<string> filePaths)
        {
            foreach (var filePath in filePaths)
                AnalyzeFile(filePath);
        }

        public int GetStringCount(string str)
        {
            return Statistics[str];
        }

        public IEnumerable<KeyValuePair<string, int>> StringFrequency => Statistics.KeyFrequency;

        public void RemoveLowFrequency(int minf)
        {
            Statistics = Statistics.Where(pair => pair.Value >= minf);
        }

        public void MergeFrom(TextAnalyzer another)
        {
            foreach (var pair in another.Statistics.KeyFrequency)
                Statistics.Add(pair.Key, pair.Value);
        }

        public void Load(string filePath)
        {
            SourceName = new FileInfo(filePath).Name;
            using (var file = File.OpenText(filePath))
                Statistics.ReadFromCsv(file, str => str);
        }
        public void Save(string filePath)
        {
            using (var file = File.CreateText(filePath))
                Statistics.WriteToCsv(file);
        }
    }
}
