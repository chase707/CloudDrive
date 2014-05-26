using Autofac;
using CloudDrive.Core;

namespace CloudDrive.Host.ConsoleHost
{
	class Program
	{
		static IContainer ApplicationContainer { get; set; }
        static void Main(string[] args)
		{
            ApplicationContainer = new CloudDriveApplicationBuilder().BuildApplication();

            var syncService = ApplicationContainer.Resolve<SyncService>();

            syncService.StartSync();

            System.Threading.Thread.CurrentThread.Join();
		}       
	}
}
