using System;
using Newtonsoft.Json.Linq;

namespace SteamBotV2.LoggingSys
{
    /// <summary>
    /// This logger logs with <see cref="Console.Write(string)"/>
    /// </summary>
    public sealed class ConsoleLogger : BaseLogger
    {
        private const ConsoleColor DEFAULT_CONSOLECOLOR = ConsoleColor.White;

        private readonly ConsoleColor defaultColor;

        /// <summary>
        /// The constructor of <see cref="ConsoleLogger"/>.
        /// </summary>
        /// <param name="configuration">Logger's configuration.</param>
        public ConsoleLogger(JObject configuration) : base(configuration)
        {
            if (ReferenceEquals(configuration, null))
            {
                defaultColor = DEFAULT_CONSOLECOLOR;
                return;
            }
            ConsoleColor configColor;
            if (!Enum.TryParse((string)configuration["DefaultColor"], out configColor))
                configColor = DEFAULT_CONSOLECOLOR;
            defaultColor = configColor;
        }

        /// <summary>
        /// This gets called by <see cref="Log"/> when it needs something logged.
        /// </summary>
        /// <param name="logging">The <see cref="Logging"/> instance containing logging information.</param>
        /// <remarks>This method is fed into a <see cref="System.Threading.Tasks.Task.Run(Action, System.Threading.CancellationToken)"/>.</remarks>
        public override void PerformLog(Logging logging)
        {
            ConsoleColor lastColor = Console.ForegroundColor;
            Console.ForegroundColor = GetColor(logging.Level);
            Console.WriteLine((string)logging);
            Console.ForegroundColor = lastColor;
        }

        private ConsoleColor GetColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Success:
                    return ConsoleColor.Green;
                case LogLevel.Warning:
                    return ConsoleColor.Yellow;
                case LogLevel.Error:
                    return ConsoleColor.Red;
                case LogLevel.Interface:
                    return ConsoleColor.Blue;
                default:
                    return defaultColor;
            }
        }
    }
}
