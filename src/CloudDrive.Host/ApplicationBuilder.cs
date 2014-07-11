using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Autofac;
using CloudDrive.Data;
using CloudDrive.Core;
using CloudDrive.Service;
using CloudDrive.Service.SkyDrive;

namespace CloudDrive.Host
{
	public class ApplicationBuilder
	{
        public IContainer BuildApplication()
		{
			var containerBuilder = new ContainerBuilder();

			containerBuilder.RegisterType<SkyDriveCloudService>()
				.WithParameter("configurationFolder", ConfigurationManager.AppSettings["CloudDrive.Core.ConfigurationFolder"])
				.As<ICloudService>()
                .SingleInstance();

            containerBuilder.RegisterType<CloudFileChangeComparer>()
                .As<ICloudFileChangeComparer>();

			containerBuilder.RegisterType<CloudDrive.Data.FileSystem.CloudUserDataSource>()
				.WithParameter("storagePath", ConfigurationManager.AppSettings["CloudDrive.Core.ConfigurationFolder"])
				.As<ICloudUserDataSource>();

            containerBuilder.Register<CloudUser>(x =>
                {
                    var cloudUserManager = x.Resolve<CloudUserManager>();
                    return cloudUserManager.Get();
                }
            ).SingleInstance();

			containerBuilder.RegisterType<CloudUserManager>();

			containerBuilder.RegisterType<CloudFileSearch>();

			containerBuilder.RegisterType<SyncQueue>()
                .SingleInstance();
            
			containerBuilder.RegisterType<FileSystemSearch>();

			containerBuilder.RegisterType<FolderWatcher>();

            containerBuilder.RegisterType<SyncService>()
                .SingleInstance();

            containerBuilder.RegisterInstance(ZeroMQ.ZmqContext.Create())
                .AsSelf()
                .SingleInstance();

            containerBuilder.RegisterType<CloudDrive.Tracing.TraceWatcher>()
                .SingleInstance();

            containerBuilder.RegisterType<CloudDrive.Tracing.TraceWriter>()
                .WithParameter("tracerAddress", ConfigurationManager.AppSettings["CloudDrive.Tracing.TraceAddress"])
                .SingleInstance();

            containerBuilder.RegisterType<CloudDrive.Tracing.TraceReader>()
                .WithParameter("tracerAddress", ConfigurationManager.AppSettings["CloudDrive.Tracing.TraceAddress"])
                .SingleInstance();

            containerBuilder.RegisterType<CloudDrive.Tracing.LogTracer>()
                .As<CloudDrive.Tracing.ITracer>()
                .WithParameter("logFilename", ConfigurationManager.AppSettings["CloudDrive.Tracing.Filename"])
                .SingleInstance();

            containerBuilder.RegisterType<CloudDrive.Tracing.ConsoleTracer>()
                .SingleInstance();

			return containerBuilder.Build();
		}
	}
}
