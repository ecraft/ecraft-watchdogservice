using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;

namespace eCraft.appFactory.appFactoryService
{
    static class Program
    {
        static int Main(string[] args)
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
            else if (Environment.UserInteractive)
            {
                if (args.Length != 1 && args.Length != 2)
                {
                    DisplayUsage();
                    return -1;
                }

                var firstArg = args[0];
                var envName = args.Length > 1 ? args[1] : null;

                switch (firstArg)
                {
                    case "/i":
                    case "/install":
                        return InstallService(envName);

                    case "/u":
                    case "/uninstall":
                        return UninstallService(envName);

                    default:
                        Console.WriteLine("Argument not recognized: {0}", args[0]);
                        Console.WriteLine(string.Empty);
                        DisplayUsage();
                        return 1;
                }
            }
            else
            {

                var servicesToRun = new ServiceBase[] { new Service() };
                ServiceBase.Run(servicesToRun);
            }

            return 0;
        }

        private static int InstallService(string environmentName)
        {
            var service = new Service();

            try
            {
                var args = new List<string>();
                if (environmentName != null) args.Add("/env=" + environmentName);
                args.Add(Assembly.GetExecutingAssembly().Location);

                // Install the service with the Windows Service Control Manager (SCM)
                ManagedInstallerClass.InstallHelper(args.ToArray());
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine();
                Console.WriteLine(
                    "InvalidOperationException: This can be caused because the process is not running with elevated privileges. " +
                    "Retry with an Administrator command prompt. Also, ensure that the service isn't already installed."
                );
                Console.WriteLine();
                Console.WriteLine(ex.ToString());
                return -1;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.GetType() == typeof(Win32Exception))
                {
                    var wex = (Win32Exception)ex.InnerException;
                    Console.WriteLine("Error(0x{0:X}): Service already installed!", wex.ErrorCode);
                    return wex.ErrorCode;
                }
                else
                {
                    Console.WriteLine(ex.ToString());
                    return -1;
                }
            }

            return 0;
        }

        private static int UninstallService(string environmentName)
        {
            try
            {
                var args = new List<string>();
                if (environmentName != null) args.Add("/env=" + environmentName);
                args.Add("/u");
                args.Add(Assembly.GetExecutingAssembly().Location);

                // uninstall the service from the Windows Service Control Manager (SCM)
                ManagedInstallerClass.InstallHelper(args.ToArray());
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine();
                Console.WriteLine(
                    "InvalidOperationException: This can be caused because the process is not running with elevated privileges. " +
                    "Retry with an Administrator command prompt. Also, ensure that the service is actually installed."
                );
                Console.WriteLine();
                Console.WriteLine(ex.ToString());
                return -1;
            }
            catch (Exception ex)
            {
                if (ex.InnerException.GetType() == typeof(Win32Exception))
                {
                    Win32Exception wex = (Win32Exception)ex.InnerException;
                    Console.WriteLine("Error(0x{0:X}): Service not installed!", wex.ErrorCode);
                    return wex.ErrorCode;
                }
                else
                {
                    Console.WriteLine(ex.ToString());
                    return -1;
                }
            }

            return 0;
        }

        private static void DisplayUsage()
        {
            Console.WriteLine("Usage: appFactoryService /i[nstall] | /u[ninstall] [env]");
            Console.WriteLine("The env is an optional parameter which makes it possible to install multiple services on the same server.");
            Console.WriteLine("Note: both the installation and the uninstallation requires elevated privileges, so they must run from an Administrator commmand prompt.");
        }
    }
}
