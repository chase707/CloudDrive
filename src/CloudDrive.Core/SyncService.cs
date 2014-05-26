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
    public delegate SyncService SyncServiceFactory(CloudUser cloudUser);

    public class SyncService
    {
        CloudUser CloudUser { get; set; }
        SyncQueue SyncQueue { get; set; }
        FileSearch FileSearch { get; set; }
        FolderWatcher FileWatcher { get; set; }
        ICloudService CloudService { get; set; }
        CloudUserManager CloudUserManager { get; set; }
        CloudFileManager CloudFileManager { get; set; }

        public SyncService(CloudUser cloudUser, SyncQueue syncQueue, FolderWatcher folderWatcher, FileSearch fileSearch, 
            CloudUserManager cloudUserManager, CloudFileManager cloudFileManager, ICloudService cloudService)
        {
            CloudUserManager = cloudUserManager;
            CloudFileManager = cloudFileManager;
            CloudUser = cloudUser;
            CloudService = cloudService;
            FileWatcher = folderWatcher;
            FileSearch = fileSearch;
            SyncQueue = syncQueue;

            // refresh folder list (find new folders since last run)
            cloudFileManager.RefreshFolders();

            // save user
            cloudUserManager.Set(CloudUser);

            // find changes against the cloud
            cloudFileManager.FindChanges();

            // enqueue any changed files
            RecursiveEnqueue(cloudUser.Files);
        }

        public void StartSync()
        {
            FileWatcher.FileChanged += FileWatcher_FileChanged;
            FileWatcher.FileCreated += FileWatcher_FileCreated;
            FileWatcher.FileDeleted += FileWatcher_FileDeleted;
            FileWatcher.FileRenamed += FileWatcher_FileRenamed;
            foreach (var file in CloudUser.Files)
            {
                FileWatcher.WatchFolder(file.LocalPath);
            }

            new System.Threading.Thread(new System.Threading.ThreadStart(SyncFileThread)).Start();
        }

        void SyncFileThread()
        {
            bool done = false;
            while (!done)
            {
                var cloudFile = SyncQueue.Dequeue();
                if (cloudFile != null)
                {
                    CloudService.Set(cloudFile);

                    CloudUserManager.Set(CloudUser);
                }
            }
        }

        void RecursiveEnqueue(IEnumerable<CloudFile> files)
        {
            foreach (var file in files)
            {
                if (file.NewOrChanged)
                    SyncQueue.Enqueue(file);

                if (file.FileType == CloudFileType.Folder)
                    RecursiveEnqueue(file.Children);
            }
        }

        void FindAndEnqueueFile(string fileName)
        {
            var cloudFile = CloudFileManager.FindFile(fileName);

            if (cloudFile == null)
            {
                cloudFile = FileSearch.FindFile(fileName);
                if (cloudFile == null)
                    return;

                var fileInfo = new System.IO.FileInfo(cloudFile.LocalPath);
                var parent = CloudFileManager.FindFile(fileInfo.DirectoryName);

                if (parent == null)
                    return;

                parent.Children.Add(cloudFile);
                cloudFile.Parent = parent;

                CloudUserManager.Set(CloudUser);
            }

            SyncQueue.Enqueue(cloudFile);
        }

        void FileWatcher_FileCreated(string fileName)
        {
            FindAndEnqueueFile(fileName);
        }

        void FileWatcher_FileChanged(string fileName)
        {
            FindAndEnqueueFile(fileName);    
        }

        void FileWatcher_FileRenamed(string oldFilename, string newFilename)
        {
            //var cloudFile = CloudFileManager.FindFile(oldFilename);

            //cloudFile.LocalPath = newFilename;
            //cloudFile
        }

        void FileWatcher_FileDeleted(string fileName)
        {
            //throw new NotImplementedException();
        }               
    }
}
