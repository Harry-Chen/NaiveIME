using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;

namespace NaiveIME
{
    interface IOption
    {

    }

    [Verb("solve", HelpText = "将拼音文件转换为汉字序列")]
    class SolveFromFileOption : IOption
    {
        [Option("in", Required = true, HelpText = "输入拼音文件")]
        public string InputFile { get; set; }

        [Option("out", Required = true, HelpText = "输入结果文件")]
        public string OutputFile { get; set; }

        [Option('m', Required = false, HelpText = "使用的模型", Default = "123l")]
        public string ModelName { get; set; }
    }

    [Verb("interactive", HelpText = "交互转换拼音")]
    class QSolveOption : IOption
    {
        [Option('m', Required = false, HelpText = "使用的模型", Default = "123l")]
        public string ModelName { get; set; }
    }

    [Verb("model", HelpText = "交互查询语言模型的概率分布")]
    class QModelOption : IOption
    {
        
    }

    [Verb("statistics", HelpText = "交互查询文本统计结果")]
    class QStatOption : IOption
    {
        [Value(0, Required = true, MetaName = "STATISTIC_FILE", HelpText = "结果文件")]
        public string FilePath { get; set; }
    }

    [Verb("analyze", HelpText = "执行文本统计")]
    class AnalyzeOption : IOption
    {
        [Value(0, MetaName = "RAW_TEXT_FILES", Required = true, HelpText = "待统计的文件")]
        public IEnumerable<string> FilePaths { get; set; }

        [Option("merge", HelpText = "是否合并统计结果。若是，需指定输出文件地址。")]
        public bool Merge { get; set; }

        [Option("out", HelpText = "输出（合并的）文件地址")]
        public string OutputFile { get; set; }

        [Option('d', HelpText = "输出目录。若不指定则为每个文件的所在目录。")]
        public string OutputDir { get; set; }

        [Option('r', Default = 0, HelpText = "保存时去掉频率低于此的项")]
        public float MinRate { get; set; }

        public string Strategy { get; set; }
    }

    [Verb("merge", HelpText = "合并统计文件")]
    class MergeOption : IOption
    {
        [Value(0, MetaName = "STATICTICS_FILES", Required = true, HelpText = "待合并的统计信息，csv格式")]
        public IEnumerable<string> FilePaths { get; set; }

        [Option('o', Required = true, HelpText = "合并后保存到的文件地址")]
        public string OutputFile { get; set; }

        [Option('r', Default = 0, HelpText = "保存时去掉频率低于此的项")]
        public float MinRate { get; set; }
    }


    [Verb("build", HelpText = "根据统计文件生成语言模型")]
    class BuildOption : IOption
    {
        [Value(0, MetaName = "STATISTICS_FILE", Required = true, HelpText = "统计信息")]
        public string StatFile { get; set; }
        [Option('m', HelpText = "模型名：1|2|3")]
        public IEnumerable<string> ModelNames { get; set; }
    }

    [Verb("test", HelpText = "在指定测试集上测试模型")]
    class TestOption : IOption
    {
        [Option('m', Required = true, HelpText = "模型名：1|2|3|12m|12l|123l")]
        public IEnumerable<string> ModelNames { get; set; }

        [Option("in", Required = true, HelpText = "输入测试集文件")]
        public string InputFile { get; set; }

        [Option("out", Required = false, HelpText = "输出结果文件")]
        public string OutputFile { get; set; }
    }

    class Program
    {
        private static void CheckConfiguration()
        {
            Console.WriteLine("----- Checking Configuration File -----");
            var lambda = PersistentConfiguration.LambdaRatio;
            if (lambda <= 0 || lambda >= 1)
            {
                throw new ArgumentException($"Mixing ratio must be in (0,1), not {lambda}");
            }
            else
            {
                Console.WriteLine($"Mixing Ratio: {lambda}");
            }
            var modelPath = PersistentConfiguration.ModelDirectory;
            if (!Directory.Exists(modelPath))
            {
                throw new FileNotFoundException($"Model directory \"{modelPath}\" does not exist");
            }
            else
            {
                Console.WriteLine($"Model directory: {modelPath}");
            }
            var takeSize = PersistentConfiguration.CandidatesEachStep;
            if (takeSize <= 0)
            {
                throw new FileNotFoundException($"Candidates taken must be positive, not {takeSize}");
            }
            else
            {
                Console.WriteLine($"Numbers of candidate taken each step: {takeSize}");

            }
            Console.WriteLine("----- Done Checking Configuration -----\n");

        }
        static void Main(string[] args)
        {
            CheckConfiguration();
            var result = CommandLine.Parser.Default.ParseArguments
                                    <QSolveOption, QModelOption, QStatOption, SolveFromFileOption, AnalyzeOption, MergeOption, BuildOption, TestOption>(args);
            result.WithParsed<QSolveOption>(opt => Command.InteractiveSolve(opt.ModelName))
                  .WithParsed<QModelOption>(opt => Command.QueryModel())
                  .WithParsed<QStatOption>(opt => Command.QueryStatistics(opt.FilePath))
                  .WithParsed<SolveFromFileOption>(opt => Command.SolveFromFile(opt.InputFile, opt.OutputFile, opt.ModelName))
                  .WithParsed<AnalyzeOption>(opt => Command.AnalyzeRawTextFiles(opt))
                  .WithParsed<MergeOption>(opt => Command.MergeStatisticsFiles(opt.FilePaths, opt.OutputFile, opt.MinRate))
                  .WithParsed<BuildOption>(opt => Command.BuildModel(opt.StatFile, opt.ModelNames))
                  .WithParsed<TestOption>(opt => Command.TestOnData(opt));

        }
    }
}
