using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace NaiveIME
{
	public static class TextProcessor
	{
	    public static float MinRate { get; set; } = 1e-6f;
	    public static string Strategy { get; set; } = "n";

	    static void DoAndPrintTime(Action action, string title = "")
		{
			if (title == "")
				title = action.ToString();
			Console.WriteLine($"Begin: {title}");
			DateTime t0 = DateTime.Now;
			action();
			TimeSpan ts = DateTime.Now - t0;
			Console.WriteLine($"End. Time = {ts}");
		}

		static void AnalyzeFiles(TextAnalyzer analyzer, IEnumerable<string> filePaths)
		{
			foreach (var filePath in filePaths)
				DoAndPrintTime(() => analyzer.AnalyzeFile(filePath),
							   $"Analyze: {filePath}");
		}

		public static void AnalyzeFiles(IEnumerable<string> filePaths, string statPath, bool append = false)
		{
			var analyzer = new TextAnalyzer();
			if(append)
				analyzer.Load(statPath);
			AnalyzeFiles(analyzer, filePaths);
			analyzer.RemoveLowFrequency((int)(analyzer.Total * MinRate));
			DoAndPrintTime(() => analyzer.Save(statPath), $"Writing to file: {statPath}");
		}

		public static void AnalyzeFilesSeparately(IEnumerable<string> filePaths, string outputDir = null)
		{
			foreach (var filePath in filePaths)
			{
				var fileInfo = new FileInfo(filePath);
				var outFilePath = $"{outputDir ?? fileInfo.DirectoryName}/{fileInfo.Name}_stat.csv";
				var analyzer = new TextAnalyzer();
				DoAndPrintTime(() => 
				{ 
					analyzer.AnalyzeFile(filePath);
					analyzer.RemoveLowFrequency((int)(analyzer.Total * MinRate));
					analyzer.Save(outFilePath);
				}, $"Analyze: {filePath}");
			}
		}

		public static void MergeFiles(IEnumerable<string> filePaths, string outFilePath)
		{
			var analyzer = new TextAnalyzer();
			foreach (var filePath in filePaths)
				DoAndPrintTime(() => analyzer.MergeFrom(new TextAnalyzer(filePath)),
				               $"Merge: {filePath}");
		    DoAndPrintTime(() => analyzer.RemoveLowFrequency((int) (analyzer.Total * MinRate)), $"Remove low frequency ({MinRate})");
			DoAndPrintTime(() => analyzer.Save(outFilePath), $"Save to {outFilePath}");
		}

		public static void RemoveLowFrequency(string statPath)
		{
			DoAndPrintTime(() =>
			{
				var analyzer = new TextAnalyzer();
				analyzer.Load(statPath);
				analyzer.RemoveLowFrequency((int)(analyzer.Total * MinRate));
				analyzer.Save(statPath);
			}, $"Remove low frequency ({MinRate}): {statPath}");
		}
	}
}
