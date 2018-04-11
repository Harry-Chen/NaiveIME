using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NaiveIME
{
    static class Command
    {
        public static void AnalyzeRawTextFiles(AnalyzeOption opt)
        {
            TextProcessor.MinRate = opt.MinRate;
            TextProcessor.Strategy = opt.Strategy;
            if (opt.Merge)
                TextProcessor.AnalyzeFiles(opt.FilePaths, opt.OutputFile);
            else
                TextProcessor.AnalyzeFilesSeparately(opt.FilePaths, opt.OutputDir);
        }

        public static void MergeStatisticsFiles(IEnumerable<string> filePaths, string outputFile, float rate = 0)
        {
            TextProcessor.MinRate = rate;
            TextProcessor.MergeFiles(filePaths, outputFile);
        }

        private static void SolveAndWritePinyin(string[] pinyins, NGramInputMethod method, TextWriter writer)
        {
            try
            {
                foreach (string pinyin in pinyins)
                    method.Input(pinyin);
            }
            catch (InvalidOperationException e)
            {
                e.ToString(); // make compiler happy
                Console.Error.WriteLine("在翻译\"" + string.Join(" ",pinyins) + "\"时发生错误，未能获取完整结果");
                var bestAnswer = method.NowBestAnswer;
                if (bestAnswer != null)
                {
                    writer.WriteLine(bestAnswer + new string('？', pinyins.Length - bestAnswer.Length));
                }
                return;
            }
            writer.WriteLine(method.Results.First());
        }

        public static void SolveFromFile(string inputFile, string outputFile, string modelName)
        {
            var model = ModelLoader.LoadByName(modelName);
            var method = new NGramInputMethod(model);
            using (var outputWriter = File.CreateText(outputFile))
            {
                var lineCount = 0;
                foreach (string input in File.ReadLines(inputFile))
                {
                    method.Clear();
                    var pinyins = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    SolveAndWritePinyin(pinyins, method, outputWriter);
                    ++lineCount;
                    if(lineCount % 10 == 0)
                    {
                        Console.Error.WriteLine($"Converted {lineCount} lines");
                    }
                }
                Console.Error.WriteLine($"Done converting pinyin, {lineCount} lines in total.");
            }
        }

        public static void InteractiveSolve(string modelName)
        {
            NGramBase model = ModelLoader.LoadByName(modelName);
            var method = new NGramInputMethod(model)
            {
                PrintDistributeSize = PersistentConfiguration.CandidatesEachStep,
            };
            Console.WriteLine($"Using {method.Name}");

            while (true)
            {
                Console.Write("Pinyin > ");
                var raw = Console.ReadLine();
                if (raw == null)
                {
                    break;
                }
                var input = raw.Trim();
                method.Clear();
                var pinyins = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                SolveAndWritePinyin(pinyins, method, Console.Out);
            }
        }

        public static void QueryModel()
        {
            NGramBase ng1 = ModelLoader.Load<NGram1>();
            NGramBase ng2 = ModelLoader.Load<NGram2>();
            NGramBase ng3 = ModelLoader.Load<NGram3>();

            while (true)
            {
                Console.Write("Data > ");
                var input = Console.ReadLine().Trim(); // [char]+ [pinyin]
                string chars = input.Split()[0];
                string pinyin = input.Split().ElementAtOrDefault(1);
                var condition = new PinyinToSolve(chars, pinyin);
                try
                {
                    Console.WriteLine("1-gram");
                    ng1.GetDistribution(condition).Take(5).Print();
                    Console.WriteLine("2-gram");
                    ng2.GetDistribution(condition).Take(5).Print();
                    Console.WriteLine("3-gram");
                    ng3.GetDistribution(condition).Take(5).Print();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }
        }

        public static void QueryStatistics(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not exist.");
                return;
            }
            var stat = new TextAnalyzer(filePath);
            while (true)
            {
                Console.Write("Statistics > ");
                var input = Console.ReadLine().Trim();
                int count = stat.GetStringCount(input);
                Console.WriteLine(count);
            }
        }

        public static void BuildModel(string filePath, IEnumerable<string> modelNames)
        {
            var stat = new TextAnalyzer(filePath);
            foreach (var modelName in modelNames)
            {
                var model = ModelLoader.NewByName(modelName);
                model.FromAnalyzer(stat);
                model.Save();
            }
        }

        public static void TestOnData(TestOption opt)
        {
            IEnumerable<NGramBase> models;
            models = opt.ModelNames.Select(ModelLoader.LoadByName);
            var inputers = models.Select(model => new NGramInputMethod(model)).Cast<SingleCharInputMethod>().ToArray();
            var tester = new InputMethodTester(inputers);

            using (var inputFile = File.OpenText(opt.InputFile))
            {
                if (opt.OutputFile == null)
                    tester.TestData(inputFile, Console.Out);
                else
                    using (var outputFile = File.CreateText(opt.OutputFile))
                        tester.TestData(inputFile, outputFile);
            }
        }
    }
}
