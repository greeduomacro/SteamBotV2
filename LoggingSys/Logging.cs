namespace SteamBotV2.LoggingSys
{
    /// <summary>
    /// This contains information supplied by <see cref="Log"/>.
    /// </summary>
    public struct Logging
    {
        /// <summary>
        /// The name of the logger that supplied <see cref="Message"/>.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// The level to class <see cref="Message"/> as.
        /// </summary>
        public LogLevel Level { get; internal set; }

        /// <summary>
        /// A culture-friendly formatted <see cref="string"/> containg actual message being logged.
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// Converts a <see cref="Logging"/> instance into a <see cref="string"/> while formatting it as a STANDARD SteamBot log string.
        /// </summary>
        /// <param name="logging"><see cref="Logging"/> to convert.</param>
        public static explicit operator string(Logging logging) => string.Format(
            Translations.Phrases.Culture,
            "{0} [{1}{2}] {3}",
            System.DateTime.Now.ToString("U", Translations.Phrases.Culture),
            logging.Level,
            string.IsNullOrWhiteSpace(logging.Name) ? "" : " " + logging.Name,
            logging.Message
            );
    }
}
