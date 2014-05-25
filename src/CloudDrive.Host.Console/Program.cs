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
        static SyncQueue SyncQueue;
        static FileSearch FileSearch = new FileSearch();
        static void Main(string[] args)
		{
			var currentUser = GetCloudUser();
			var refreshedUser = new CloudUser(currentUser.UniqueName);			
			var skyDriveService = new SkyDriveCloudService(ConfigurationManager.AppSettings["CloudDrive.Core.ConfigurationFolder"]);
            var fileComparer = new CloudFileChangeComparer(skyDriveService);

            SyncQueue = new Core.SyncQueue(currentUser, fileComparer);

            // find any new/changed local files
            // currently deletes are not supported
            foreach (var rootFolder in currentUser.Files)
            {
                var foundFile = FileSearch.FindFilesAndFolders(rootFolder.LocalPath);
                if (foundFile != null)
                    refreshedUser.Files.Add(foundFile);
            }

            // enqueue any changes for sync
            foreach (var file in refreshedUser.Files)
            {
                SyncQueue.EnqueueFileTree(file);
            }

            var fileWatcher = new FolderWatcher();
            fileWatcher.WatchFolder(currentUser.Files[0].LocalPath);
            fileWatcher.FileChanged += fileWatcher_FileChanged;
            System.Threading.Thread.CurrentThread.Join();


            var thread = new System.Threading.Thread();
            //fileSync.SyncLocalFolder();

            //currentUser.Files = refreshedUser.Files;

            //SaveCloudUser(currentUser);
		}

        static void fileWatcher_FileChanged(string fileName)
        {
            var cloudFile = FileSearch.FindFile(fileName);

            SyncQueue.EnqueueFile(cloudFile);
        }

        static void SyncFileThread()
        {

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
