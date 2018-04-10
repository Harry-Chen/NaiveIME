using Microsoft.Extensions.Configuration;
using System.IO;

namespace NaiveIME
{
    public static class PersistentConfiguration
    {
        private static IConfiguration config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("config.json").Build();
        public static string ModelDirectory => config["ModelDirectory"];
        public static float LambdaRatio => float.Parse(config["LambdaRatio"]);
        public static int CandidatesEachStep => int.Parse(config["CandidatesEachStep"]);
    }
}