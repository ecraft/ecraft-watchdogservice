using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace eCraft.appFactory.appFactoryService
{
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        private ServiceProcessInstaller serviceProcessInstaller;
        private ServiceInstaller serviceInstaller;

        public ProjectInstaller()
        {
            InitializeComponent();
        }

        // Based on a hint from http://journalofasoftwaredev.wordpress.com/2008/07/16/multiple-instances-of-same-windows-service/.
        public override void Install(IDictionary stateSaver)
        {
            RetrieveServiceName();
            base.Install(stateSaver);
        }

        public override void Uninstall(IDictionary savedState)
        {
            RetrieveServiceName();
            base.Uninstall(savedState);
        }

        private void RetrieveServiceName()
        {
            var environment = Context.Parameters["env"];

            if (String.IsNullOrEmpty(environment))
            {
                // No environment name has been provided. We resort to the default names.
                return;
            }

            serviceInstaller.ServiceName += "_" + environment;
            serviceInstaller.DisplayName += String.Format(" ({0})", environment);
        }

        private void InitializeComponent()
        {
            serviceProcessInstaller = new ServiceProcessInstaller();
            serviceInstaller = new ServiceInstaller();

            // 
            // serviceProcessInstaller
            // 
            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            serviceProcessInstaller.Password = null;
            serviceProcessInstaller.Username = null;

            // 
            // serviceInstaller
            // 
            serviceInstaller.Description = "eCraft Watchdog Service. This service ensures that all necessary processes " +
                "are running";
            serviceInstaller.DisplayName = "eCraft appFactory";
            serviceInstaller.ServiceName = "appFactory";
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            // 
            // ProjectInstaller
            // 
            Installers.AddRange(new Installer[] {
                serviceProcessInstaller,
                serviceInstaller
            });
        }
    }
}
