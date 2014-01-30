﻿using System.Collections.Generic;
using Autofac;
using CloudDrive.Core;
using CloudDrive.Data;
using CloudDrive.Service;

namespace CloudDrive.Host.ConsoleHost
{
	class Program
	{
		static IContainer ApplicationContainer { get; set; }
		static void Main(string[] args)
		{
			ApplicationContainer = new CloudDriveApplicationBuilder().BuildApplication();

			var cloudUserManager = ApplicationContainer.Resolve<CloudUserManager>();
			var fileSearch = ApplicationContainer.Resolve<FileSearch>();
			var cacheManagerFactory = ApplicationContainer.Resolve<CacheFileManagerFactory>();
			var cloudService = ApplicationContainer.Resolve<ICloudService>();
			
			var currentUser = cloudUserManager.Get("chase707@gmail.com");
			var refreshedUser = new CloudUser(currentUser.UniqueName);			

			// iterate through root folders and grab new list of files
			foreach (var rootFolder in currentUser.Files)
			{
				var foundFile = fileSearch.FindFilesAndFolders(rootFolder.LocalPath);
				if (foundFile != null)
					refreshedUser.Files.Add(foundFile);
			}

			// find differences between cache and current files on disk/cloud
			var cacheManager = cacheManagerFactory(currentUser);
			cacheManager.EvalDifferences(refreshedUser.Files);
			
			// sync differences
			foreach (var file in refreshedUser.Files)
				RecursiveSync(cloudService, refreshedUser.Files);

			// save out user info
			currentUser.Files = refreshedUser.Files;
			cloudUserManager.Set(currentUser);
		}

		static void RecursiveSync(ICloudService cloudService,  IEnumerable<CloudFile> files, CloudFile parentFile = null)
		{
			foreach (var file in files)
			{
				if (file.NewOrChanged)
					cloudService.Set(parentFile != null ? parentFile.RemoteId : string.Empty, file);

				if (file.FileType == CloudFileType.Folder)
					RecursiveSync(cloudService, file.Children, file);
			}
		}
	}
}
