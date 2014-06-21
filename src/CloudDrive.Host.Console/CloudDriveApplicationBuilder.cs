﻿using System;
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

			containerBuilder.RegisterType<CloudFileManager>();

			containerBuilder.RegisterType<SyncQueue>()
                .SingleInstance();
            
			containerBuilder.RegisterType<FileSearch>();

			containerBuilder.RegisterType<FolderWatcher>();

            containerBuilder.RegisterType<SyncService>()
                .SingleInstance();

			return containerBuilder.Build();
		}
	}
}
