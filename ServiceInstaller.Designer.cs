/****************************** Module Header ******************************
 * Module Name:  CorpMailman Windows Service project.
 * Project:      CorpMailman: Windows Service to send out emails project.
 *
 * Partial ServiceInstaller class that provides Dispose and InitializeComponent methods.
 *
 * Revisions:
 *     1. Sundar Krishnamurthy         sundar_k@hotmail.com               8/27/2015       Initial file created.
***************************************************************************/

namespace CorpMailman {

    #region Using directives
    using System.Diagnostics;
    using System.Security.Principal;
    using System.ServiceProcess;
    #endregion

    #region Partial ServiceInstaller class
    /// <summary>
    /// Partial ServiceInstaller class that provides Dispose and InitializeComponent methods.
    /// </summary>
    partial class ServiceInstaller {

        #region Members
        /// <summary>Required designer variable.</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>ServiceInstaller instance to register and manage service</summary>
        private System.ServiceProcess.ServiceInstaller serviceInstaller;

        /// <summary>ServiceProcessInstaller instance to manage installation of service</summary>
        private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller;
        #endregion

        #region Methods
        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {

            if (disposing && (this.components != null)) {
                this.components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {

            // Construct components as a Container instance
            this.components = new System.ComponentModel.Container();

            this.serviceInstaller = new System.ServiceProcess.ServiceInstaller();
            this.serviceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();

            // serviceInstaller
            this.serviceInstaller.Description = Constants.System.ServiceDesc;
            this.serviceInstaller.DisplayName = Constants.System.NtServiceName;
            this.serviceInstaller.ServiceName = Constants.System.ServiceKey;
            this.serviceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;

            // serviceProcessInstaller - uncomment the next line and comment the one after if you need to run this with a regular user account
            // Setting this up as a NetworkService makes it run under a Service account, with email sent via a service account too - password in CorpMailman.exe.config
//            this.serviceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.User;
            this.serviceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.NetworkService;

            // ServiceInstaller
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
                this.serviceProcessInstaller,
                this.serviceInstaller
            });

            // Add handler to automatically start service on install
            serviceInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(serviceInstaller_AfterInstall);
        }

        #region Event Handlers
        /// <summary>
        /// After install, start the service
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">InstallEventArgs object</param>
        private void serviceInstaller_AfterInstall(object sender, System.Configuration.Install.InstallEventArgs e) {

            // Get current privilege
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            // Create event source if it does not exist, and we have administrator privileges
            if (principal.IsInRole(WindowsBuiltInRole.Administrator)) {
                if (!EventLog.SourceExists(Constants.System.NtServiceName)) {
                    EventLog.CreateEventSource(Constants.System.NtServiceName, Constants.System.Application);
                }
            }

            ServiceController sc = new ServiceController(Constants.System.NtServiceName);
            sc.Start();
        }
        #endregion
        #endregion
        #endregion
    }
    #endregion
}