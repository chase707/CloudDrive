using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CloudDrive.Host.ServiceHost
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
			if (System.Environment.UserInteractive)
			{
				new CloudDriveService().StartService();

				System.Threading.Thread.CurrentThread.Join();
			}
			else
			{
				System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);

				ServiceBase[] ServicesToRun;
				ServicesToRun = new ServiceBase[] 
				{ 
					new CloudDriveService() 
				};
				ServiceBase.Run(ServicesToRun);
			}
        }
    }
}
