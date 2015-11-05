using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SteamBotV2.LoggingSys
{
    using ModuleSys;

    internal sealed class LoggerLoader : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) => objectType == typeof(IEnumerable<BaseLogger>);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            IList<BaseLogger> loggers = new List<BaseLogger>();
            JArray array = JArray.Load(reader);
            foreach (JObject obj in array)
            {
                string dllName = (string)obj["DLL"];
                if (string.IsNullOrWhiteSpace(dllName))
                    continue;
                Assembly reqAssembly = ModuleManager.FindModule(dllName);
                if (reqAssembly == null)
                    continue;
                foreach (JObject lObj in (JArray)obj["Types"])
                {
                    string typeName = (string)lObj["Type"];
                    if (loggers.Any(logger => logger.GetType().FullName.Equals(typeName, StringComparison.CurrentCulture)))
                    {
                        Console.WriteLine(Translations.Phrases.logger_loaded, typeName);
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(typeName))
                        continue;
                    Type reqType = reqAssembly.GetType(typeName);
                    if (reqType == null)
                    {
                        Console.WriteLine(Translations.Phrases.logger_missing, typeName, dllName);
                        continue;
                    }
                    else if (!typeof(BaseLogger).IsAssignableFrom(reqType))
                    {
                        Console.WriteLine(Translations.Phrases.invalid_type, typeName);
                        continue;
                    }
                    try
                    {
                        BaseLogger logger = Activator.CreateInstance(reqType, lObj["Params"] as JObject) as BaseLogger;
                        if (!ReferenceEquals(logger, null))
                            loggers.Add(logger);
                    }
                    catch (Exception) { }
                }
            }
            return loggers;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { throw new NotSupportedException(Translations.Phrases.write_unsupported); }
    }
}