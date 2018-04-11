using System;
using System.Collections.Generic;
using System.Linq;

namespace NaiveIME
{
    /// <summary>
    /// 使用N-Gram统计模型的完整拼音输入法。
    /// </summary>
    public class NGramInputMethod : SingleCharInputMethod
    {
        public override string Name => $"[{Model.GetType().Name}][{Model.SourceName}]";
        NGramBase Model { get; }
        Distribution<string> distribution = Distribution<string>.Single("");
        List<string> goodResults = new List<string>();
        public int TakeSize { get; set; } = PersistentConfiguration.CandidatesEachStep;

        public bool TraceDistribute { get; set; } = false;
        public bool MakeGoodResults { get; set; } = false;
        public int PrintDistributeSize { get; set; } = 0;
        public List<Distribution<string>> Distributions { get; } = new List<Distribution<string>>();

        public NGramInputMethod(NGramBase model)
        {
            Model = model;
        }

        public override IEnumerable<string> Results
            => distribution.KeyProbDescending.Select(pair => pair.Key);

        public override IEnumerable<string> SubResult
            => goodResults.Reverse<string>();

        private string longestAnswer;

        public override string NowBestAnswer => longestAnswer;

        public override void Clear()
        {
            base.Clear();
            distribution = Distribution<string>.Single("");
            goodResults.Clear();
            Distributions.Clear();
        }

        public override void Input(string pinyin)
        {
            distribution = distribution.ExpandAndMerge(str =>
                    Model.GetDistribution(new PinyinToSolve(str, pinyin))
                        .Take(TakeSize)
                        .Select(result => str + result.Substring(0, 1)))
                    .Take(TakeSize)
                    .Norm();

            if (MakeGoodResults)
                goodResults.AddRange(distribution.KeyProbDescending
                                             .TakeWhile(pair => pair.Value > 0.2)
                                             .Reverse()
                                             .Select(pair => pair.Key));
            if (TraceDistribute)
                Distributions.Add(distribution);
            if (PrintDistributeSize > 0)
                distribution.Take(PrintDistributeSize).Print();

            longestAnswer = distribution.KeyProbDescending.First().Key;
        }

        public override void ConfirmSubResult(int index)
        {
            throw new NotImplementedException();
        }
    }

    public static class NGramInputeMethodExtension
    {
        public static NGramInputMethod GetInputMethod(this NGramBase model)
        {
            return new NGramInputMethod(model);
        }
    }
}
