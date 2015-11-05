using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SteamBotV2.LoggingSys
{
    /// <summary>
    /// This logger logs with <see cref="TextWriter.WriteLine(string)"/> on an open file.
    /// </summary>
    public sealed class FileLogger : BaseLogger, IDisposable
    {
        private const int DEFAULT_CLEANDAYS = 7;

        private readonly string baseFileName;
        private readonly Thread rotaterThread;
        private readonly bool rotateLogs;
        
        private StreamWriter fileWriter;
        private DateTime dateOpened;
        private bool runThread;
        private bool disposed;
        private object myLock = new object();

        /// <summary>
        /// The constructor of <see cref="ConsoleLogger"/>.
        /// </summary>
        /// <param name="configuration">Logger's configuration.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="configuration"/> is null.<para/>
        /// -or-<para/>
        /// <paramref name="configuration"/>["LogFile"] is null.
        /// </exception>
        public FileLogger(JObject configuration) : base(configuration)
        {
            if (ReferenceEquals(configuration, null))
                throw new ArgumentNullException(nameof(configuration));
            baseFileName = (string)configuration["LogFile"];
            if (string.IsNullOrWhiteSpace(baseFileName))
                throw new ArgumentNullException(nameof(configuration) + "[\"LogFile\"]");
            bool configRotate, configClean;
            int configLife;
            if (!bool.TryParse((string)configuration["Rotate"], out configRotate))
                configRotate = true;
            if (!bool.TryParse((string)configuration["Clean"], out configClean))
                configClean = true;
            if (!int.TryParse((string)configuration["CleanDays"], out configLife))
                configLife = DEFAULT_CLEANDAYS;
            rotateLogs = configRotate;
            if (!Directory.Exists("logs"))
                Directory.CreateDirectory("logs");
            if (rotateLogs && configClean)
                Task.Run(() =>
                {
                    DateTime now = DateTime.Now;
                    foreach (string path in Directory.GetFiles("logs").Where(path => File.GetCreationTime(path) < now.AddDays(-configLife)))
                        File.Delete(path);
                });
            runThread = true;
            rotaterThread = new Thread(() =>
            {
                dateOpened = DateTime.Now;
                fileWriter = new StreamWriter(Path.Combine("logs", $"{baseFileName}{(rotateLogs ? $"-{dateOpened.ToString("dd-MM-yyyy")})" : "")}.log"), true, Encoding.UTF8);
                fileWriter.AutoFlush = true;
                while (rotateLogs && runThread)
                {
                    DateTime now = DateTime.Now;
                    if (dateOpened < now.AddDays(-1))
                    {
                        lock (myLock)
                        {
                            fileWriter.Dispose();
                            dateOpened = now;
                            fileWriter = new StreamWriter(Path.Combine("logs", $"{baseFileName}-{dateOpened.ToString("dd-MM-yyyy")}.log"), true, Encoding.UTF8);
                            fileWriter.AutoFlush = true;
                        }
                    }
                    if (runThread)
                        Thread.Sleep(new TimeSpan(TimeSpan.TicksPerMinute));
                }
            });
            rotaterThread.IsBackground = true;
            rotaterThread.Start();
        }

        /// <summary>
        /// Finalize method for <see cref="FileLogger"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Logger was disposed of.</exception>
        ~FileLogger() { Dispose(false); }

        /// <summary>
        /// This gets called by <see cref="Log"/> when it needs something logged.
        /// </summary>
        /// <param name="logging">The <see cref="Logging"/> instance containing logging information.</param>
        /// <remarks>This method is fed into a <see cref="System.Threading.Tasks.Task.Run(Action, System.Threading.CancellationToken)"/>.</remarks>
        public override void PerformLog(Logging logging) { lock(myLock) { fileWriter.WriteLine(logging); } }

        private void Dispose(bool disposing)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(FileLogger));
            runThread = false;
            if (disposing)
                lock(myLock) { fileWriter.Dispose(); }
            disposed = true;
        }

        /// <summary>
        /// This disposes of the underlying <see cref="StreamWriter"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Logger was disposed of.</exception>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
