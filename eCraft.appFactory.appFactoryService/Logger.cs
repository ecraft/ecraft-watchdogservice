using System;
using System.Diagnostics;
using System.IO;

namespace eCraft.appFactory.appFactoryService
{
    static class Logger
    {
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
    }
}
