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

namespace CloudDrive.Host.ConsoleHost
{
	public class CloudDriveApplicationBuilder
	{
		public IContainer BuildApplication()
		{
			var containerBuilder = new ContainerBuilder();

			containerBuilder.RegisterType<SkyDriveCloudService>()
				.WithParameter("configurationFolder", ConfigurationManager.AppSettings["CloudDrive.Core.ConfigurationFolder"])
				.As<ICloudService>();

			containerBuilder.RegisterType<CloudFileDateComparison>()
				.As<ICloudFileComparison>();

			containerBuilder.RegisterType<CloudDrive.Data.FileSystem.CloudUserDataSource>()
				.WithParameter("storagePath", ConfigurationManager.AppSettings["CloudDrive.Core.ConfigurationFolder"])
				.As<ICloudUserDataSource>();

			containerBuilder.RegisterType<CloudUserManager>();

			containerBuilder.RegisterType<CacheFileManager>();

			containerBuilder.RegisterType<CloudFileSyncQueue>();

			containerBuilder.RegisterType<FileSearch>();

			containerBuilder.RegisterType<FolderWatcher>();

			return containerBuilder.Build();
		}
	}
}
