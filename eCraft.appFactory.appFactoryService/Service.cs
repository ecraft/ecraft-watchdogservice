using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace eCraft.appFactory.appFactoryService
{
    public partial class Service : ServiceBase
    {
        const int CTRL_C_EVENT = 0;
        const int CTRL_BREAK_EVENT = 1;

        [DllImport("kernel32.dll")]
        static extern bool GenerateConsoleCtrlEvent(
            uint dwCtrlEvent,
            uint dwProcessGroupId);

        private readonly List<Process> processes;
        private List<Application> applications;

        public Service()
        {
            InitializeComponent();
            processes = new List<Process>();
        }

        public void Run()
        {
            try
            {
                Logger.Log("Watchdog service starting");
                Logger.DeleteOldLogs();
                applications = ConfigParser.GetApplicationsToStart();
                applications.ForEach(StartApplication);
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                throw;
            }
        }

        protected override void OnStart(string[] args)
        {
            Run();
        }

        protected override void OnStop()
        {
            Logger.Log("appFactory service stopping, trying to stop running applications");
            
            for(int i = processes.Count -1; i >= 0; i--)
            {
                var proc = processes[i];
                processes.Remove(proc);
                Logger.Log(String.Format("Stopping application {0} with PID {1}", proc.StartInfo.FileName, proc.Id));
                StopProcess(proc);
            }
        }

        void StartApplication(Application app)
        {
            var proc = new Process();
            processes.Add(proc);
            proc.StartInfo.FileName = app.FileName;
            proc.StartInfo.Arguments = string.Join(" ", app.Arguments.ToArray());
            //proc.StartInfo.CreateNoWindow = true;
            foreach (var env in app.EnvironmentVariables)
            {
                Logger.Log(String.Format("Adding environment variable {0} with value {1}", env.Key, env.Value));

                proc.StartInfo.EnvironmentVariables[env.Key] = env.Value;
            }
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;

            ConfigureProcess(proc, app);

            Logger.Log(string.Format("Starting application {0} with arguments {1}", app.FileName, string.Join(" ", app.Arguments.ToArray())));

            try
            {
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                BackOffHandler.RegisterProcessStart(proc.StartInfo);
                Logger.Log(string.Format("Application {0} started with PID {1}", app.FileName, proc.Id));
            }
            catch (Win32Exception e)
            {
                // FIXME: What is the correct action to take here?
                Logger.Log(string.Format("Application {0} failed to start: {1}. Terminating service.", app.FileName, e.Message));

                throw new Exception("One or more applications failed to start. Service terminating.", e);
            }
        }

        private void ConfigureProcess(Process proc, Application app)
        {
            proc.EnableRaisingEvents = true;
            proc.ErrorDataReceived += OnProcessErrorDataReceived;
            proc.OutputDataReceived += OnProcessOutputDataReceived;
            proc.Exited += OnProcessExited;

            app.Process = proc;
        }

        void StopProcess(Process proc)
        {
            try
            {
                proc.CloseMainWindow();
                var ret = GenerateConsoleCtrlEvent(CTRL_C_EVENT, (uint)proc.SessionId);
                proc.StandardInput.Close(); 
                Logger.Log(string.Format("CTRL-C event sent to process {0} and returned {1}", proc.SessionId, ret));
                if (!proc.HasExited)
                {
                    Logger.Log(string.Format("Waiting for {0} to close", proc.StartInfo.FileName));
                    if (!proc.WaitForExit(10 * 1000))
                    {
                        Logger.Log(string.Format("Application {0} did not stop voluntarily. Will try to kill it.", proc.StartInfo.FileName));
                        proc.Kill();
                    }
                }
                proc.Close();
            }
            catch (Exception e)
            {
                Logger.Log(string.Format("Caught exception while trying to close appliction {0}: {1}", proc.StartInfo.FileName, e.Message));
            }
        }

        void OnProcessExited(object sender, EventArgs e)
        {
            var proc = sender as Process;
            
            if (proc == null)
            { 
                return; 
            }

            try
            {
                Logger.Log(string.Format("Application {0} with PID {1} has exited with exitcode {2}", proc.StartInfo.FileName, proc.Id, proc.ExitCode));
                
                if (ShouldProcessBeRestarted(proc))
                {
                    RestartProcess(proc);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("Caught exception in proc_exited {0}: {1}", proc.StartInfo.FileName, ex.Message));
            }
        }

        bool ShouldProcessBeRestarted(Process proc)
        {
            return processes.Contains(proc);
        }

        void RestartProcess(Process proc)
        {
            Logger.Log(string.Format("Trying to restart application {0}", proc.StartInfo.FileName));
            BackOffHandler.WaitAppropriateAmountOfTime(proc.StartInfo);
            
            if (ShouldProcessBeRestarted(proc))
            {
                // It seems like we cannot capture output to the stdout or stderr when we restart the process (we get no events),
                // and calling BeginOutputReadLine fails, so... we create a new process object altogether.
                processes.Remove(proc);
                var app = GetApplication(proc);

                proc = new Process
                {
                    StartInfo = proc.StartInfo
                };

                ConfigureProcess(proc, app);
                processes.Add(proc);

                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();

                BackOffHandler.RegisterProcessStart(proc.StartInfo);
                Logger.Log(string.Format("Application {0} restarted with PID {1}", proc.StartInfo.FileName, proc.Id));
            }
        }

        private void OnProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data))
            {
                var proc = sender as Process;
                
                if (proc == null)
                { 
                    return; 
                }

                var app = GetApplication(proc);
                Logger.LogStandardOutput(Path.GetFileName(proc.StartInfo.FileName), proc.Id, app == null ? null : app.Identifier, e.Data);
            }
        }

        private void OnProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data))
            {
                var proc = sender as Process;
                
                if (proc == null)
                { 
                    return; 
                }

                var app = GetApplication(proc);
                Logger.LogStandardError(Path.GetFileName(proc.StartInfo.FileName), proc.Id, app == null ? null : app.Identifier, e.Data);
            }
        }

        private Application GetApplication(Process proc)
        {
            foreach (var application in applications)
            {
                if (application.Process == proc)
                {
                    return application;
                }
            }

            return null;
        }
    }
}
