namespace CloudDrive.Host.ServiceHost
{
    partial class ProjectInstaller
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
            cloudDriveServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            cloudDriveServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // cloudDriveServiceProcessInstaller
            // 
            cloudDriveServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalService;
            cloudDriveServiceProcessInstaller.Password = null;
            cloudDriveServiceProcessInstaller.Username = null;
            // 
            // cloudDriveServiceInstaller
            // 
            cloudDriveServiceInstaller.DelayedAutoStart = true;
            cloudDriveServiceInstaller.ServiceName = "CloudDrive";
            cloudDriveServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            Installers.AddRange(new System.Configuration.Install.Installer[] {
            cloudDriveServiceProcessInstaller,
            cloudDriveServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller cloudDriveServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller cloudDriveServiceInstaller;
    }
}