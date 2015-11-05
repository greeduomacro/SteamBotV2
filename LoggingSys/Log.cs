using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SteamBotV2.LoggingSys
{
    /// <summary>
    /// The main class of LoggingSys that performs the logging.
    /// </summary>
    public sealed class Log : IDisposable
    {
        private readonly string name;
        private readonly bool show;
        private readonly IEnumerable<BaseLogger> loggers;

        private bool disposed;
        private object myLock = new object();

        internal Log(string n, bool s, IEnumerable<BaseLogger> l)
        {
            name = n;
            show = s;
            loggers = l;
        }

        /// <summary>
        /// Finalize method for <see cref="Log"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Logger was disposed of.</exception>
        ~Log() { Dispose(false); }

        private void DoLogging(LogLevel level, string format, object[] args)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(Log));
            else if (ReferenceEquals(format, null))
                throw new ArgumentNullException(nameof(format));
            else if (ReferenceEquals(format, null))
                throw new ArgumentNullException(nameof(args));
            Logging logging = new Logging()
            {
                Name = show ? name : null,
                Level = level,
                Message = string.Format(Translations.Phrases.Culture, format, args)
            };
            lock(myLock)
            {
                foreach (BaseLogger logger in loggers.Where(logger => level >= logger.MinimumLevel))
                    Task.Run(() => logger.PerformLog(logging), CancellationToken.None);
            }
        }

        /// <summary>
        /// This logs a message at <see cref="LogLevel.Debug"/>.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <exception cref="ArgumentNullException"><paramref name="format"/> or <paramref name="args"/> is null.</exception>
        /// <exception cref="FormatException">
        /// <paramref name="format"/> is invalid.<para/>
        /// -or-<para/>
        /// The index of a format item is less than zero, or greater than or equal to the length of the <paramref name="args"/> array.
        /// </exception>
        /// <exception cref="ObjectDisposedException">Logger was disposed of.</exception>
        public void Debug(string format, params object[] args) => DoLogging(LogLevel.Debug, format, args);

        /// <summary>
        /// This logs a message at <see cref="LogLevel.Info"/>.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <exception cref="ArgumentNullException"><paramref name="format"/> or <paramref name="args"/> is null.</exception>
        /// <exception cref="FormatException">
        /// <paramref name="format"/> is invalid.<para/>
        /// -or-<para/>
        /// The index of a format item is less than zero, or greater than or equal to the length of the <paramref name="args"/> array.
        /// </exception>
        /// <exception cref="ObjectDisposedException">Logger was disposed of.</exception>
        public void Info(string format, params object[] args) => DoLogging(LogLevel.Info, format, args);

        /// <summary>
        /// This logs a message at <see cref="LogLevel.Success"/>.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <exception cref="ArgumentNullException"><paramref name="format"/> or <paramref name="args"/> is null.</exception>
        /// <exception cref="FormatException">
        /// <paramref name="format"/> is invalid.<para/>
        /// -or-<para/>
        /// The index of a format item is less than zero, or greater than or equal to the length of the <paramref name="args"/> array.
        /// </exception>
        /// <exception cref="ObjectDisposedException">Logger was disposed of.</exception>
        public void Success(string format, params object[] args) => DoLogging(LogLevel.Success, format, args);

        /// <summary>
        /// This logs a message at <see cref="LogLevel.Warning"/>.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <exception cref="ArgumentNullException"><paramref name="format"/> or <paramref name="args"/> is null.</exception>
        /// <exception cref="FormatException">
        /// <paramref name="format"/> is invalid.<para/>
        /// -or-<para/>
        /// The index of a format item is less than zero, or greater than or equal to the length of the <paramref name="args"/> array.
        /// </exception>
        /// <exception cref="ObjectDisposedException">Logger was disposed of.</exception>
        public void Warning(string format, params object[] args) => DoLogging(LogLevel.Warning, format, args);

        /// <summary>
        /// This logs a message at <see cref="LogLevel.Error"/>.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <exception cref="ArgumentNullException"><paramref name="format"/> or <paramref name="args"/> is null.</exception>
        /// <exception cref="FormatException">
        /// <paramref name="format"/> is invalid.<para/>
        /// -or-<para/>
        /// The index of a format item is less than zero, or greater than or equal to the length of the <paramref name="args"/> array.
        /// </exception>
        /// <exception cref="ObjectDisposedException">Logger was disposed of.</exception>
        public void Error(string format, params object[] args) => DoLogging(LogLevel.Error, format, args);

        /// <summary>
        /// This logs a message at <see cref="LogLevel.Interface"/>.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <exception cref="ArgumentNullException"><paramref name="format"/> or <paramref name="args"/> is null.</exception>
        /// <exception cref="FormatException">
        /// <paramref name="format"/> is invalid.<para/>
        /// -or-<para/>
        /// The index of a format item is less than zero, or greater than or equal to the length of the <paramref name="args"/> array.
        /// </exception>
        /// <exception cref="ObjectDisposedException">Logger was disposed of.</exception>
        public void Interface(string format, params object[] args) => DoLogging(LogLevel.Interface, format, args);

        private void Dispose(bool disposing)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(Log));
            if (disposing)
            {
                lock(myLock)
                {
                    foreach (IDisposable disposable in loggers.OfType<IDisposable>())
                        disposable.Dispose();
                }
            }
            disposed = true;
        }

        /// <summary>
        /// This disposes of all <see cref="BaseLogger"/> that inherit <see cref="IDisposable"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Logger was disposed of.</exception>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
