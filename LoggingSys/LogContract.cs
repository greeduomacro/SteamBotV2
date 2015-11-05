using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace SteamBotV2.LoggingSys
{
    /// <summary>
    /// This provides a contract for creating a <see cref="Log"/> instance from JSON.
    /// </summary>
    public struct LogContract
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate, PropertyName = "Name")]
        [DefaultValue("System")]
        internal string Name { get; private set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate, PropertyName = "Show")]
        [DefaultValue(true)]
        internal bool Show { get; private set; }

        [JsonProperty("Loggers")]
        [JsonConverter(typeof(LoggerLoader))]
        internal IEnumerable<BaseLogger> Loggers { get; private set; }

        /// <summary>
        /// Converts a <see cref="LogContract"/> into a <see cref="Log"/>.
        /// </summary>
        /// <param name="contract">Contract to convert.</param>
        public static implicit operator Log(LogContract contract) => new Log(contract.Name, contract.Show, contract.Loggers);
    }
}
