using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace eCraft.appFactory.appFactoryService
{
    static class ConfigParser
    {
        public static List<Application> GetApplicationsToStart()
        {
            var applications = new List<Application>();
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            var configPath = Path.GetFullPath(dir + "/run.config");

            if (Debugger.IsAttached)
            {
                configPath = "service.config";
            }

            Logger.Log(string.Format("Enumerating applications to start from the '{0}' file", configPath));

            var configDoc = new XmlDocument();
            configDoc.Load(configPath);

            var workingDirectory = configDoc.DocumentElement.Attributes["WorkingDirectory"].Value.Replace("%cd%", dir);

            Environment.CurrentDirectory = workingDirectory;
            Logger.Log(string.Format(
                "Working directory for service set to '{0}'. All relative paths will be relative to this directory.", Environment.CurrentDirectory));

            XmlNodeList applicationElements = configDoc.SelectNodes("/Applications/Application");
            foreach (XmlNode applicationElement in applicationElements)
            {
                var app = new Application()
                {
                    FileName = applicationElement.Attributes["FileName"].Value
                };

                var identifier = applicationElement.Attributes["Identifier"];
                app.Identifier = identifier?.Value;

                XmlNodeList environmentVariableNodes = applicationElement.SelectNodes("EnvironmentVariable");
                foreach (XmlNode environmentVariable in environmentVariableNodes)
                {
                    var key = environmentVariable.Attributes["Key"].Value;
                    var value = environmentVariable.Attributes["Value"].Value.Replace("%cd%", workingDirectory);
                    app.EnvironmentVariables.Add(key, value);
                }

                XmlNodeList argumentNodes = applicationElement.SelectNodes("Argument");
                foreach (XmlNode argument in argumentNodes)
                {
                    var value = argument.Attributes["Value"].Value.Replace("%cd%", workingDirectory);
                    app.Arguments.Add(value);
                }
                applications.Add(app);
            }

            Logger.Log(String.Format("Found {0} applications.", applications.Count));
            return applications;
        }
    }
}
