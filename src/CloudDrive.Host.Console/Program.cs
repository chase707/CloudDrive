using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SkyNet;
using SkyNet.Client;
using CloudDrive.Service.SkyDrive;
using CloudDrive.Core;
using CloudDrive.Data;
using CloudDrive.Data.FileSystem;

namespace CloudDrive.Host.ConsoleHost
{
	class Program
	{		
		static void Main(string[] args)
		{
			var currentUser = GetCloudUser();
			var refreshedUser = new CloudUser(currentUser.UniqueName);
			var fileSearch = new FileSearch();
			var skyDriveService = new SkyDriveCloudService(ConfigurationManager.AppSettings["CloudDrive.Core.ConfigurationFolder"]);
			var fileSync = new FileSyncService(skyDriveService, currentUser);

			// iterate through root folders and grab new list of files
			foreach (var rootFolder in currentUser.Files)
			{
				var foundFile = fileSearch.FindFilesAndFolders(rootFolder.LocalPath);
				if (foundFile != null)
					refreshedUser.Files.Add(foundFile);
			}

			fileSync.SyncFolder(refreshedUser.Files);

			currentUser.Files = refreshedUser.Files;

			SaveCloudUser(currentUser);
		}

		static CloudUser GetCloudUser()
		{
			CloudUserDataSource dataSource = new CloudUserDataSource(ConfigurationManager.AppSettings["CloudDrive.Core.ConfigurationFolder"]);
			var myCloudUser = dataSource.Get(string.Empty);
			if (myCloudUser == null)
			{
				myCloudUser = new CloudUser("chase707@gmail.com");
				dataSource.Set(myCloudUser);
			}

			return myCloudUser;
		}

		static void SaveCloudUser(CloudUser myUser)
		{
			CloudUserDataSource dataSource = new CloudUserDataSource(ConfigurationManager.AppSettings["CloudDrive.Core.ConfigurationFolder"]);
			dataSource.Set(myUser);
		}		
	}
}
