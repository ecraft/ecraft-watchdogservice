using System;
using System.Diagnostics;

namespace eCraft.appFactory.appFactoryService
{
    class RestartTime
    {
        public ProcessStartInfo ProcessStartInfo { get; set; }
        public DateTime StartTime { get; set; }

        public RestartTime(ProcessStartInfo startInfo)
        {
            ProcessStartInfo = startInfo;
            StartTime = DateTime.Now;
        }
    }
}
