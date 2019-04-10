using System;
using System.Diagnostics;
using System.IO;

namespace eCraft.appFactory.appFactoryService
{
    static class Logger
    {
        const int DaysToKeepLogs = 60;
        static object loggingLock = new object();

        internal const string EventSourceName = "eCraft Watchdog Service";

        public static void Log(string message)
        {
            var logName = String.Format("{0}{1}-{2}.log", GetLogFolder(), "service",
                                        DateTime.UtcNow.ToString("yyyy-MM-dd"));
            AppendToLog(logName, message);
        }

        public static void LogStandardOutput(string executableFileName, int processId, string identifier, string message)
        {
            // In honor of the Unix file descriptor numbers, 1 == stdout.
            LogHelper(executableFileName, processId, identifier, "[1] " + message);
        }

        public static void LogStandardError(string executableFileName, int processId, string identifier, string message)
        {
            // In honor of the Unix file descriptor numbers, 2 == stderr.
            LogHelper(executableFileName, processId, identifier, "[2] " + message);
        }

        private static void LogHelper(string executableFileName, int processId, string identifier, string message)
        {
            var logName = String.Format(
                "{0}{1}{2}-{3}-{4}.log",
                GetLogFolder(),
                (identifier == null ? String.Empty : identifier + "-"),
                DateTime.UtcNow.ToString("yyyyMMdd"),
                executableFileName,
                processId
            );
            AppendToLog(logName, message);
        }

        private static string GetLogFolder()
        {
            if (Debugger.IsAttached)
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                return AppDomain.CurrentDomain.BaseDirectory + "../logs/";
            }
        }

        private static void AppendToLog(string logFileName, string message)
        {
            // A very primitive approach to locking, but the KISS approach for now. It turned out that if two threads concurrently
            // write to the same file, we run into problems; the output from one of the threads get lost (?) if we don't handle
            // this. I thought about using a queuing mechanism here, and have a logging thread pop the queue and write the entries
            // to file sequentially, but I opted for this extremely simplistic approach for now.
            lock (loggingLock)
            {
                try
                {
                    using (var fs = File.AppendText(logFileName))
                    {
                        var output = String.Format("{0} {1}", DateTime.UtcNow.ToString("o"), message);
                        fs.WriteLine(output);
                        Trace.WriteLine(output);
                    }
                }
                catch (Exception e)
                {
                    WriteEventLog(e);
                }
            }
        }

        private static void WriteEventLog(Exception e)
        {
            if (!EventLog.SourceExists(EventSourceName))
            {
                EventLog.CreateEventSource(EventSourceName, "Application");
            }
            EventLog.WriteEntry(EventSourceName, "Error occurred (AppendToLog):" + e, EventLogEntryType.Error);
        }

        public static void DeleteOldLogs()
        {
            Log(String.Format("Removing logs older than {0} days.", DaysToKeepLogs));

            var cutoffTime = DateTime.Now.AddDays(-DaysToKeepLogs);
            var filesDeleted = 0;
            var files = GetLogFiles();

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);

                if (fileInfo.CreationTime < cutoffTime)
                {
                    try
                    {
                        fileInfo.Delete();
                        filesDeleted++;
                    }
                    catch (Exception exc)
                    {
                        Log(String.Format("Error deleting {0}.\n{1}", file, exc));
                    }
                }
            }

            Log(String.Format("Removed {0} files.", filesDeleted));
        }

        private static string[] GetLogFiles()
        {
            var logpath = GetLogFolder();
            var files = Directory.GetFiles(logpath);
            return files;
        }
    }
}
