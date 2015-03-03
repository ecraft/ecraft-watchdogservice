using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;

namespace eCraft.appFactory.appFactoryService
{
    static class Program
    {
        static void Main()
        {
            if (Debugger.IsAttached)
            {
                // Debug code: this allows the process to run as a non-service. It will kick off the service start point, but
                // never kill it. Shut down the debugger to exit the process.
                var service = new Service();
                service.Run();

                // Put a breakpoint on the following line to always catch the service when it has finished its work
                Thread.Sleep(Timeout.Infinite);
            }
            else
            {

                var servicesToRun = new ServiceBase[] { new Service() };
                ServiceBase.Run(servicesToRun);
        }
    }
}
}
