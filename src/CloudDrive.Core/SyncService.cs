using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CloudDrive.Core;
using CloudDrive.Data;
using CloudDrive.Data.FileSystem;
using CloudDrive.Service;

namespace CloudDrive.Core
{
    public class SyncService
    {
        CloudUser CloudUser { get; set; }
        SyncQueue SyncQueue { get; set; }
        FileSearch FileSearch { get; set; }
        FolderWatcher FileWatcher { get; set; }
        ICloudService CloudService { get; set; }
        CloudUserDataSource CloudUserDataSource { get; set; }

        public SyncService(CloudUserDataSource dataSource, ICloudService cloudService, ICloudFileChangeComparer fileComparer)
        {
            CloudUserDataSource = dataSource;
            CloudUser = GetCloudUser();
            CloudService = cloudService; 
            FileWatcher = new FolderWatcher();
            FileSearch = new FileSearch();
            SyncQueue = new SyncQueue(CloudUser, fileComparer);            
        }

        public CloudUser RefreshCloudUser()
        {
            var refreshedUser = new CloudUser(CloudUser.UniqueName);
            
            // find any new/changed local files
            // currently deletes are not supported
            foreach (var rootFolder in CloudUser.Files)
            {
                var foundFile = FileSearch.FindFilesAndFolders(rootFolder.LocalPath);
                if (foundFile != null)
                    refreshedUser.Files.Add(foundFile);
            }

            CloudUser.Files = refreshedUser.Files;

            SaveCloudUser();

            return CloudUser;
        }

        public void StartSync()
        {
            FileWatcher.FileChanged += FileWatcher_FileChanged;

            foreach (var file in CloudUser.Files)
            {
                SyncQueue.EnqueueFileTree(file);
                FileWatcher.WatchFolder(file.LocalPath);
            }

            new System.Threading.Thread(new System.Threading.ThreadStart(SyncFileThread)).Start();
        }

        void FileWatcher_FileChanged(string fileName)
        {
            var cloudFile = FileSearch.FindFile(fileName);
            if (cloudFile == null)
                return;

            SyncQueue.EnqueueFile(cloudFile);
        }

        void SyncFileThread()
        {
            bool done = false;
            while (!done)
            {
                var cloudFile = SyncQueue.Dequeue();
                if (cloudFile != null)
                {
                    CloudService.Set(cloudFile.Parent, cloudFile);
                }
            }
        }

        CloudUser GetCloudUser()
        {            
            var myCloudUser = CloudUserDataSource.Get(string.Empty);
            //if (myCloudUser == null)
            //{
            //    myCloudUser = new CloudUser("chase707@gmail.com");
            //    dataSource.Set(myCloudUser);
            //}

            return myCloudUser;
        }

        void SaveCloudUser()
        {
            CloudUserDataSource.Set(CloudUser);
        }
    }
}
