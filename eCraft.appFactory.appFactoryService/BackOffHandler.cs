using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace eCraft.appFactory.appFactoryService
{
    static class BackOffHandler
    {
        static readonly List<RestartTime> restartTimes;

        static BackOffHandler()
        {
            restartTimes = new List<RestartTime>();
        }

        public static void RegisterProcessStart(ProcessStartInfo startInfo)
        {
            restartTimes.Add(new RestartTime(startInfo));
        }

        public static void WaitAppropriateAmountOfTime(ProcessStartInfo startInfo)
        {
            var restartCount = 0;
            foreach (var restartTime in restartTimes)
            {
                if (restartTime.ProcessStartInfo != startInfo) continue;
                if (restartTime.StartTime < DateTime.Now.AddMinutes(-5)) continue;
                restartCount++;
            }

            if (restartCount == 0) return;

            var secondsToWait = 5 * restartCount;
            secondsToWait = ConstrainToRange(2, 5 * 60, secondsToWait);
            Thread.Sleep(secondsToWait * 1000);
        }

        static int ConstrainToRange(int min, int max, int value)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
}
