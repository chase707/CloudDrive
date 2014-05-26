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
<<<<<<< HEAD
<<<<<<< HEAD
				.As<ICloudService>()
                .SingleInstance();

            containerBuilder.RegisterType<CloudFileChangeComparer>()
                .As<ICloudFileChangeComparer>();
=======
=======
>>>>>>> f8d26e4d8c6b8cdb3423c6b36280233a24eb9515
				.As<ICloudService>();

			containerBuilder.RegisterType<CloudFileDateComparison>()
				.As<ICloudFileComparison>();
<<<<<<< HEAD
>>>>>>> Refactored FileSync into CacheFileManager and Comparison, external sync
=======
>>>>>>> f8d26e4d8c6b8cdb3423c6b36280233a24eb9515

			containerBuilder.RegisterType<CloudDrive.Data.FileSystem.CloudUserDataSource>()
				.WithParameter("storagePath", ConfigurationManager.AppSettings["CloudDrive.Core.ConfigurationFolder"])
				.As<ICloudUserDataSource>();

<<<<<<< HEAD
<<<<<<< HEAD
            containerBuilder.Register<CloudUser>(x =>
                {
                    var cloudUserManager = x.Resolve<CloudUserManager>();
                    return cloudUserManager.Get("chase707@gmail.com");
                }
            ).SingleInstance();

			containerBuilder.RegisterType<CloudUserManager>();

			containerBuilder.RegisterType<CloudFileManager>();

			containerBuilder.RegisterType<SyncQueue>()
                .SingleInstance();
=======
=======
>>>>>>> f8d26e4d8c6b8cdb3423c6b36280233a24eb9515
			containerBuilder.RegisterType<CloudUserManager>();

			containerBuilder.RegisterType<CacheFileManager>();

			containerBuilder.RegisterType<CloudFileSyncQueue>();
<<<<<<< HEAD
>>>>>>> Refactored FileSync into CacheFileManager and Comparison, external sync
=======
>>>>>>> f8d26e4d8c6b8cdb3423c6b36280233a24eb9515

			containerBuilder.RegisterType<FileSearch>();

			containerBuilder.RegisterType<FolderWatcher>();

<<<<<<< HEAD
<<<<<<< HEAD
            containerBuilder.RegisterType<SyncService>()
                .SingleInstance();

=======
>>>>>>> Refactored FileSync into CacheFileManager and Comparison, external sync
=======
>>>>>>> f8d26e4d8c6b8cdb3423c6b36280233a24eb9515
			return containerBuilder.Build();
		}
	}
}
