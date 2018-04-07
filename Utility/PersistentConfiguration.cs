using Microsoft.Extensions.Configuration;

namespace NaiveIME
{
    public static class PersistentConfiguration
    {
        private static IConfiguration config = new ConfigurationBuilder().AddJsonFile("config.json").Build();
        public static string DefaultPinyinFile => config["DefaultPinyinFile"];
        public static string ModelDirectory => config["ModelDirectory"];
    }
}