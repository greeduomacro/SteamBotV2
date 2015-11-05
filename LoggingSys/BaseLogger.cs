using System;
using Newtonsoft.Json.Linq;

namespace SteamBotV2.LoggingSys
{
    /// <summary>
    /// The base class of all loggers for SteamBotV2.
    /// </summary>
    public abstract class BaseLogger
    {
        private const LogLevel DEFAULT_LOGLEVEL = LogLevel.Info;

        internal readonly LogLevel MinimumLevel;

        /// <summary>
        /// The base constructor of all loggers.
        /// </summary>
        /// <param name="configuration">Logger's configuration.</param>
        public BaseLogger(JObject configuration)
        {
            if (ReferenceEquals(configuration, null))
            {
                MinimumLevel = DEFAULT_LOGLEVEL;
                return;
            }
            LogLevel configLevel;
            if (!Enum.TryParse((string)configuration["LogLevel"], out configLevel))
                configLevel = DEFAULT_LOGLEVEL;
            MinimumLevel = configLevel;
        }

        /// <summary>
        /// This gets called by <see cref="Log"/> when it needs something logged.
        /// </summary>
        /// <param name="logging">The <see cref="Logging"/> instance containing logging information.</param>
        /// <remarks>This method is fed into a <see cref="System.Threading.Tasks.Task.Run(Action, System.Threading.CancellationToken)"/>.</remarks>
        public abstract void PerformLog(Logging logging);
    }
}
