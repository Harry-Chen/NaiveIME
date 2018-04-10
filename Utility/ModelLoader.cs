using Newtonsoft.Json;
using System;
using System.IO;

namespace NaiveIME
{
    public static class ModelLoader
    {
        static string GetPath(Type type)
        {
            return Path.Combine(PersistentConfiguration.ModelDirectory, $"{type.Name}.txt");
        }

        public static NGramBase NewByName(string modelName)
        {
            switch (modelName)
            {
                case "1": return new NGram1();
                case "2": return new NGram2();
                case "3": return new NGram3();
                default: throw new ArgumentException();
            }
        }

        public static NGramBase LoadByName(string modelName)
        {
            switch (modelName)
            {
                case "1": return Load<NGram1>();
                case "2": return Load<NGram2>();
                case "3": return Load<NGram3>();
                case "12m": return Load12WithMaxProbability();
                case "12l": return Load12WithCoefficients();
                case "123l": return Load123WithCoefficients();
                default: throw new ArgumentException();
            }
        }

        public static TModel Load<TModel>()
            where TModel : NGramBase
        {
            TModel model;
            var modelPath = GetPath(typeof(TModel));
            using (var fileReader = File.OpenText(modelPath))
            {
                Console.WriteLine($"Loading {typeof(TModel).Name} from {modelPath}...");
                model = new JsonSerializer().Deserialize<TModel>(new JsonTextReader(fileReader));
                Console.WriteLine($"Success.");
            }
            return model;
        }

        public static void Save<TModel>(this TModel model)
            where TModel : NGramBase
        {
            using (var fileWriter = File.CreateText(GetPath(model.GetType())))
            {
                Console.WriteLine($"Saving {model.GetType().Name} to file...");
                new JsonSerializer().Serialize(fileWriter, model);
                Console.WriteLine("Success.");
            }
        }

        public static NGramMixed Load12WithMaxProbability()
        {
            var ng1 = Load<NGram1>();
            var ng2 = Load<NGram2>();
            var model = new NGramMixed(new NGramBase[] { ng1, ng2 })
            {
                SourceName = "12m"
            };
            return model;
        }


        public static NGramMixed Load12WithCoefficients()
        {
            var ng1 = Load<NGram1>();
            var ng2 = Load<NGram2>();

            var model = new NGramMixed(new NGramBase[] { ng1, ng2 })
            {
                MixDistributeStrategy = NGramMixed.MixStrategyCoefficient,
                SourceName = "12l"
            };
            return model;
        }

        public static NGramMixed Load123WithCoefficients()
        {
            var ng1 = Load<NGram1>();
            var ng2 = Load<NGram2>();
            var ng3 = Load<NGram3>();
            var model = new NGramMixed(new NGramBase[] { ng1, ng2, ng3 })
            {
                MixDistributeStrategy = NGramMixed.MixStrategyCoefficient,
                SourceName = "123l"
            };
            return model;
        }
    }
}
