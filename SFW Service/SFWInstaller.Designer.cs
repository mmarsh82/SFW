
namespace SFW_Service
{
    partial class SFWInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.sfwServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.sfwServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // sfwServiceProcessInstaller
            // 
            this.sfwServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.sfwServiceProcessInstaller.Password = null;
            this.sfwServiceProcessInstaller.Username = null;
            this.sfwServiceProcessInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.sfwServiceProcessInstaller_AfterInstall);
            // 
            // sfwServiceInstaller
            // 
            this.sfwServiceInstaller.Description = "Updates Dispatch SQL components";
            this.sfwServiceInstaller.DisplayName = "Dispatch SQL Service";
            this.sfwServiceInstaller.ServiceName = "SFWService";
            this.sfwServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            this.sfwServiceInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.sfwServiceInstaller_AfterInstall);
            // 
            // SFWInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.sfwServiceProcessInstaller,
            this.sfwServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller sfwServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller sfwServiceInstaller;
    }
}