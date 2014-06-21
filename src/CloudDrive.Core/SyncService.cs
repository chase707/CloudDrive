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
            var refreshedUser = cloudFileManager.RefreshUser();
            
            // find changes against the cloud
            cloudFileManager.FindChanges(refreshedUser);

            // save out changes
            CloudUser = refreshedUser;
            CloudUserManager.Set(CloudUser);

            // enqueue any changed files
            RecursiveEnqueue(CloudUser.Files);
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
                var syncItem = SyncQueue.Dequeue();
                if (syncItem != null)
                {
                    switch (syncItem.RequestedOperation)
                    {
                        case SyncQueueItem.SyncOperation.Save:
                            CloudService.Set(syncItem.CloudFile);
                            break;
                        case SyncQueueItem.SyncOperation.Rename:
                            CloudService.Rename(syncItem.CloudFile, (string) syncItem.OperationData);
                            break;
                        case SyncQueueItem.SyncOperation.None:
                        case SyncQueueItem.SyncOperation.Delete:
                            break;
                    }
                    CloudUserManager.Set(CloudUser);
                }
            }
        }

        void RecursiveEnqueue(IEnumerable<CloudFile> files)
        {
            foreach (var file in files)
            {
                if (file.NewOrChanged)
                    SyncQueue.Enqueue(new SyncQueueItem()
                    {
                        CloudFile = file,
                        OperationData = null,
                        RequestedOperation = SyncQueueItem.SyncOperation.Save
                    });

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

            SyncQueue.Enqueue(new SyncQueueItem()
            {
                CloudFile = cloudFile,
                OperationData = null,
                RequestedOperation = SyncQueueItem.SyncOperation.Save
            });
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
            // This is a problem because the file could be currently queued to sync
            // and it was renamed.
            // So we need to figure out a way to track the sync and rename after
            var cloudFile = CloudFileManager.FindFile(oldFilename);
            if (cloudFile != null)
            {
                SyncQueue.Enqueue(new SyncQueueItem()
                {
                    CloudFile = cloudFile,
                    OperationData = newFilename,
                    RequestedOperation = SyncQueueItem.SyncOperation.Rename
                });
            }
        }

        void FileWatcher_FileDeleted(string fileName)
        {
            // delete not currently supported
        }               
    }
}
