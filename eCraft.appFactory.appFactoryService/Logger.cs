using System;
using System.Diagnostics;
using System.IO;

namespace eCraft.appFactory.appFactoryService
{
    static class Logger
    {
        const int DaysToKeepLogs = 60;

        public static void Log(string message)
        {
            var logName = String.Format("{0}{1}-{2}.log", GetLogFolder(), "service",
                                        DateTime.UtcNow.ToString("yyyy-MM-dd"));
            AppendToLog(logName, message);
        }

        public static void LogStandardOutput(string executableFileName, int processId, string identifier, string message)
        {
            LogHelper(executableFileName, processId, identifier, "output", message);
        }

        public static void LogStandardError(string executableFileName, int processId, string identifier, string message)
        {
            LogHelper(executableFileName, processId, identifier, "error", message);
        }

        private static void LogHelper(string executableFileName, int processId, string identifier, string fileNameSuffix,
                                      string message)
        {
            var logName = String.Format("{0}{1}{2}-{3}-{4}.{5}", GetLogFolder(), (identifier == null ? String.Empty : identifier + "-"),
                                        DateTime.UtcNow.ToString("yyyyMMdd"), executableFileName, processId, fileNameSuffix);
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
                return AppDomain.CurrentDomain.BaseDirectory + "..\\..\\logs\\service\\";
            }
        }

        private static void AppendToLog(string logName, string message)
        {
            using (var fs = File.AppendText(logName))
            {
                var output = String.Format("{0} {1}", DateTime.UtcNow.ToString("o"), message);
                fs.WriteLine(output);
                Trace.WriteLine(output);
            }
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
