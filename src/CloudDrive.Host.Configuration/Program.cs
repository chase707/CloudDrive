using System;
using System.Configuration;
using System.Windows.Forms;
using CloudDrive.Service.SkyDrive;

namespace CloudDrive.Host.Configuration
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            var cloudService = new SkyDriveCloudService(ConfigurationManager.AppSettings["CloudDrive.Core.ConfigurationFolder"]);
            if (!cloudService.Authorized)
                Application.Run(new SkyDriveSignInForm(cloudService));
            
            Application.Run(new MainForm());
        }
    }
}
