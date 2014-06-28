using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CloudDrive.Data;
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

        public SyncService(SyncQueue syncQueue, FolderWatcher folderWatcher, FileSearch fileSearch,
            CloudUserManager cloudUserManager, CloudFileManager cloudFileManager, ICloudService cloudService)
        {
            CloudUserManager = cloudUserManager;
            CloudFileManager = cloudFileManager;
            CloudService = cloudService;
            FileWatcher = folderWatcher;
            FileSearch = fileSearch;
            SyncQueue = syncQueue;

            CloudUser = CloudUserManager.Get();

            CoreApp.TraceWriter.Trace("Refreshing Folders...");
            
            // refresh folder list (find new folders since last run)
            var allFiles = cloudFileManager.FindAllFiles(CloudUser.Files);

            CoreApp.TraceWriter.Trace("Finding Changes...");
            // find changes against the cloud, which will mark files new/changed
            cloudFileManager.FindChanges(CloudUser.Files, allFiles);

            CloudUser.Files = allFiles.ToList();

            // save out changes
            CloudUserManager.Set(CloudUser);

            CoreApp.TraceWriter.Trace("Enqueueing Changes...");
            // enqueue any changed files
            RecursiveEnqueue(CloudUser.Files);
        }

        public void Start()
        {
            CoreApp.TraceWriter.Trace("Starting Folder Watcher...");
            FileWatcher.FileChanged += FileWatcher_FileChanged;
            FileWatcher.FileCreated += FileWatcher_FileCreated;
            FileWatcher.FileDeleted += FileWatcher_FileDeleted;
            FileWatcher.FileRenamed += FileWatcher_FileRenamed;
            foreach (var file in CloudUser.Files)
            {
                FileWatcher.WatchFolder(file.LocalPath);
            }

            CoreApp.TraceWriter.Trace("Starting Sync Queue...");
            new System.Threading.Thread(new System.Threading.ThreadStart(SyncFileThread)).Start();
        }

        void SyncFileThread()
        {
            try
            {
                bool done = false;
                while (!done)
                {
                    var syncItem = SyncQueue.Dequeue();
                    if (syncItem != null)
                    {
                        CoreApp.TraceWriter.Trace("File Dequeued: {0}", syncItem.CloudFile.LocalPath);                        
                        switch (syncItem.RequestedOperation)
                        {
                            case SyncQueueItem.SyncOperation.Save:
                                {
                                    CoreApp.TraceWriter.Trace(string.Format("Syncing File: {0}", syncItem.CloudFile.LocalPath));
                                    CloudService.Set(syncItem.CloudFile);
                                    CoreApp.TraceWriter.Trace(string.Format("Syncing File Complete: {0}", syncItem.CloudFile.LocalPath));
                                }
                                break;
                            case SyncQueueItem.SyncOperation.Rename:
                                {
                                    CoreApp.TraceWriter.Trace(string.Format("Renaming File: {0}", syncItem.CloudFile.LocalPath));
                                    CloudService.Rename(syncItem.CloudFile);
                                    CoreApp.TraceWriter.Trace(string.Format("Renaming File Complete: {0}", syncItem.CloudFile.LocalPath));
                                }
                                break;
                            case SyncQueueItem.SyncOperation.None:
                            case SyncQueueItem.SyncOperation.Delete:
                                break;
                        }
                        lock (this)
                        {
                            CloudUserManager.Set(CloudUser);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CoreApp.TraceWriter.Trace("SyncQueue Exception: {0}", ex.ToString());
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

        void FindAndEnqueueFile(CloudFileType fileType, string fileName)
        {
            CoreApp.TraceWriter.Trace("Finding CloudFile: {0}", fileName);
            CloudFile cloudFile = null;
            lock (this)
            { 
                cloudFile = CloudFileManager.FindFile(CloudUser.Files, fileName);
            }
            if (cloudFile == null)
            {
                CoreApp.TraceWriter.Trace("CloudFile not found, searching filesystem...");
                cloudFile = TryAndFindFile(fileType, fileName);
                if (cloudFile == null)
                {
                    CoreApp.TraceWriter.Trace("File not found on filesystem");
                    return;
                }

                var fileInfo = new System.IO.FileInfo(cloudFile.LocalPath);
                CoreApp.TraceWriter.Trace("Finding parent: {0}", fileInfo.DirectoryName);
                CloudFile parent = null;
                lock (this)
                {
                    parent = CloudFileManager.FindFile(CloudUser.Files, fileInfo.DirectoryName);
                }
                if (parent == null)
                {
                    CoreApp.TraceWriter.Trace("Parent not found");
                    return;
                }

                parent.Children.Add(cloudFile);
                cloudFile.Parent = parent;

                lock (this)
                {
                    CloudUserManager.Set(CloudUser);
                }
            }
            else
            {
                CoreApp.TraceWriter.Trace("CloudFile Found: {0}", fileName);
            }

            if (cloudFile == null) return;

            SyncQueue.Enqueue(new SyncQueueItem()
            {
                CloudFile = cloudFile,
                OperationData = null,
                RequestedOperation = SyncQueueItem.SyncOperation.Save
            });
        }

        void FileWatcher_FileCreated(CloudFileType fileType, string fileName)
        {
            try
            {
                FindAndEnqueueFile(fileType, fileName);
            }
            catch (Exception ex)
            {
                CoreApp.TraceWriter.Trace(string.Format("Exception: {0}", ex.ToString()));
            }
        }

        void FileWatcher_FileChanged(CloudFileType fileType, string fileName)
        {
            try
            {
                FindAndEnqueueFile(fileType, fileName);
            }
            catch (Exception ex)
            {
                CoreApp.TraceWriter.Trace(string.Format("Exception: {0}", ex.ToString()));
            }
        }

        void FileWatcher_FileRenamed(CloudFileType fileType, string oldFilename, string newFilename)
        {
            try
            {
                var newFileNoPath = Path.GetFileName(newFilename);
                lock (this)
                {
                    var cloudFile = CloudFileManager.FindFile(CloudUser.Files, oldFilename);
                    if (cloudFile != null)
                    {
                        cloudFile.LocalPath = newFilename;
                        cloudFile.Name = newFileNoPath;
                        CloudUserManager.Set(CloudUser);

                        SyncQueue.Enqueue(new SyncQueueItem()
                        {
                            CloudFile = cloudFile,
                            OperationData = oldFilename,
                            RequestedOperation = SyncQueueItem.SyncOperation.Rename
                        });

                        return;
                    }
                }
                
                FindAndEnqueueFile(fileType, newFilename);
            }
            catch (Exception ex)
            {
                CoreApp.TraceWriter.Trace(string.Format("Exception: {0}", ex.ToString()));
            }
        }

        void FileWatcher_FileDeleted(CloudFileType fileType, string fileName)
        {
            // remove from cache
            lock (this)
            {
                var cloudFile = CloudFileManager.FindFile(CloudUser.Files, fileName);
                if (cloudFile == null) return;

                if (cloudFile.Parent != null)
                {
                    cloudFile.Parent.Children.Remove(cloudFile);
                    cloudFile.Parent = null;
                }

                CloudUserManager.Set(CloudUser);
            }
        }

        CloudFile TryAndFindFile(CloudFileType fileType, string fileName)
        {
            CoreApp.TraceWriter.Trace("Trying to Find File/Folder on FileSystem {0}", fileName);
            const int maxTrys = 20;
            const int delay = 100;
            var trys = 0;
            // loop until the file is found -
            // a new file/folder could be renamed before it gets added to the cloud file manager
            CloudFile cloudFile = null;
            while (cloudFile == null && trys < maxTrys)
            {
                cloudFile = fileType == CloudFileType.File ? FileSearch.FindFile(fileName) : FileSearch.FindFolder(fileName);
                if (cloudFile != null)
                {
                    CoreApp.TraceWriter.Trace("File/Folder Found on FileSystem : {0}", fileName);

                    return cloudFile;
                }
                else
                {
                    System.Threading.Thread.Sleep(delay);
                    trys++;
                }
            }

            return null;
        }
    }
}
