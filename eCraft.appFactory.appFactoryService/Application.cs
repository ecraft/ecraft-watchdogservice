using System.Collections.Generic;
using System.Diagnostics;

namespace eCraft.appFactory.appFactoryService
{
    class Application
    {
        public string FileName { get; set; }
        public Dictionary<string, string> EnvironmentVariables { get; set; }
        public List<string> Arguments { get; set; }
        public string Identifier { get; set; }
        public Process Process { get; set; }

        public Application()
        {
            EnvironmentVariables = new Dictionary<string, string>();
            Arguments = new List<string>();
        }
    }
}
