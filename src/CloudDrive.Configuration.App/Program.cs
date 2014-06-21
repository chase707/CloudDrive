using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CloudDrive.Service.SkyDrive;
using System.Configuration;

namespace CloudDrive.Configuration.App
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

            var path = Application.ExecutablePath;

            var config = ConfigurationManager.OpenExeConfiguration(string.Format("{0}\\..\\var\\app.config", path));

            var cloudService = new SkyDriveCloudService(ConfigurationManager.AppSettings["CloudDrive.Core.ConfigurationFolder"]);
            if (!cloudService.Authorized)
                Application.Run(new SkyDriveSignInForm(cloudService));
            
            Application.Run(new MainForm());
        }
    }
}
