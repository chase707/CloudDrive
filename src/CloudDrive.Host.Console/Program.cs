using Autofac;
using CloudDrive.Core;
using CloudDrive.Tracing;

namespace CloudDrive.Host.ConsoleHost
{
	class Program
	{
        static void Main(string[] args)
		{
            var appHost = new ApplicationHost();

            var logTracer = appHost.AppContainer.Resolve<ITracer>();
            var consoleTracer = appHost.AppContainer.Resolve<ConsoleTracer>();

            var syncService = appHost.AppContainer.Resolve<SyncService>();
            syncService.Start();

            System.Threading.Thread.CurrentThread.Join();
		}
	}
}
