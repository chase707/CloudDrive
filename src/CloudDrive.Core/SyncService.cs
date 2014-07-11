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
        CloudUser User { get; set; }
        CloudUserManager UserManager { get; set; }
        SyncQueue FileSyncQueue { get; set; }
        FileSystemSearch FileSearch { get; set; }
        FolderWatcher FileWatcher { get; set; }
        ICloudService CloudService { get; set; }
        CloudFileSearch CloudSearch { get; set; }
        ICloudFileChangeComparer FileComparison { get; set; }

        static object CloudFileLock = new object();

        public SyncService(SyncQueue syncQueue, FolderWatcher folderWatcher, FileSystemSearch fileSearch,
            CloudUserManager cloudUserManager, CloudFileSearch cloudSearch, ICloudService cloudService, ICloudFileChangeComparer fileComparison)
        {
            UserManager = cloudUserManager;
            CloudSearch = cloudSearch;
            CloudService = cloudService;
            FileWatcher = folderWatcher;
            FileSearch = fileSearch;
            FileComparison = fileComparison;
            FileSyncQueue = syncQueue;

            FileWatcher.FileChanged += FileWatcher_FileChanged;
            FileWatcher.FileCreated += FileWatcher_FileCreated;
            FileWatcher.FileDeleted += FileWatcher_FileDeleted;
            FileWatcher.FileRenamed += FileWatcher_FileRenamed;

            CloudSearch.FileMatched += CloudFileSearch_FileMatch;
            CloudSearch.NewFile += CloudFileSearch_NewFile;
            CloudSearch.DeletedFile += CloudFileSearch_FileDeleted;

            User = UserManager.Get();
        }

        public void Start()
        {
            CoreApp.TraceWriter.Trace("Starting Folder Watcher...");

            foreach (var file in User.Files)
            {
                FileWatcher.WatchFolder(file.LocalPath);
            }

            CoreApp.TraceWriter.Trace("Finding Changes Since Last Run...");

            CloudSearch.MatchFiles(User.Files);

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
                    var syncItem = FileSyncQueue.Dequeue();
                    if (syncItem != null)
                    {
                        CoreApp.TraceWriter.Trace("File Dequeued: {0}", syncItem.CloudFile.LocalPath);
                        switch (syncItem.RequestedOperation)
                        {
                            case SyncQueueItem.SyncOperation.Save:
                                {
                                    CoreApp.TraceWriter.Trace(string.Format("Syncing File: {0}", syncItem.CloudFile.LocalPath));
                                    var remoteFile = CloudService.Set(syncItem.CloudFile);

                                    lock (CloudFileLock)
                                    {
                                        // replace updated file in tree
                                        if (syncItem.CloudFile.Parent != null)
                                        {
                                            remoteFile.Parent = syncItem.CloudFile.Parent;
                                            remoteFile.Parent.Children.Add(remoteFile);
                                            remoteFile.Parent.Children.Remove(syncItem.CloudFile);
                                            syncItem.CloudFile.Parent = null;
                                        }
                                        else // replace root item
                                        {
                                            User.Files.Remove(syncItem.CloudFile);
                                            User.Files.Add(remoteFile);
                                        }

                                        if (syncItem.CloudFile.Children != null)
                                        {
                                            foreach (var child in syncItem.CloudFile.Children)
                                            {
                                                child.Parent = remoteFile;
                                                remoteFile.Children.Add(child);
                                            }

                                            syncItem.CloudFile.Children.Clear();
                                        }

                                        syncItem.CloudFile = null;

                                        UserManager.Set(User);
                                    }

                                    CoreApp.TraceWriter.Trace(string.Format("Syncing File Complete: {0}", remoteFile.LocalPath));
                                }
                                break;
                            case SyncQueueItem.SyncOperation.Rename:
                                {
                                    lock (CloudFileLock)
                                    {
                                        CoreApp.TraceWriter.Trace(string.Format("Renaming File: {0}", syncItem.CloudFile.LocalPath));
                                        CloudService.Rename(syncItem.CloudFile);
                                        UserManager.Set(User);
                                        CoreApp.TraceWriter.Trace(string.Format("Renaming File Complete: {0}", syncItem.CloudFile.LocalPath));
                                    }
                                }
                                break;
                            case SyncQueueItem.SyncOperation.None:
                            case SyncQueueItem.SyncOperation.Delete:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CoreApp.TraceWriter.Trace("SyncQueue Exception: {0}", ex.ToString());
            }
        }

        void FindAndEnqueueFile(CloudFileType fileType, string fileName)
        {
            CoreApp.TraceWriter.Trace("Finding CloudFile: {0}", fileName);
            CloudFile cloudFile = null;
            lock (CloudFileLock)
            {
                cloudFile = CloudSearch.FindFile(User.Files, fileName);
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

                    parent = CloudSearch.FindFile(User.Files, fileInfo.DirectoryName);

                    if (parent == null)
                    {
                        CoreApp.TraceWriter.Trace("Parent not found");
                        return;
                    }

                    parent.Children.Add(cloudFile);
                    cloudFile.Parent = parent;

                    UserManager.Set(User);
                }
                else
                {
                    CoreApp.TraceWriter.Trace("CloudFile Found: {0}", fileName);
                }
            }

            if (cloudFile == null) return;

            FileSyncQueue.Enqueue(new SyncQueueItem()
            {
                CloudFile = cloudFile,
                OperationData = null,
                RequestedOperation = SyncQueueItem.SyncOperation.Save
            });
        }

        void RemoveCloudFile(CloudFileType fileType, string fileName)
        {
            // remove from cache
            lock (CloudFileLock)
            {
                var cloudFile = CloudSearch.FindFile(User.Files, fileName);
                if (cloudFile == null) return;

                if (cloudFile.Parent != null)
                {
                    cloudFile.Parent.Children.Remove(cloudFile);
                    cloudFile.Parent = null;
                }

                UserManager.Set(User);
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
        
        void CloudFileSearch_FileDeleted(CloudFile deletedFile)
        {
            CoreApp.TraceWriter.Trace("FileDeleted: {0}", deletedFile.LocalPath);

            RemoveCloudFile(deletedFile.FileType, deletedFile.LocalPath);
        }

        CloudFile CloudFileSearch_NewFile(CloudFile parentFolder, CloudFile newFile)
        {
            CoreApp.TraceWriter.Trace("Newfile: {0}, Parent: {1}", newFile.LocalPath, parentFolder.LocalPath);

            lock (CloudFileLock)
            {
                if (parentFolder != null)
                {
                    parentFolder.Children.Add(newFile);
                    newFile.Parent = parentFolder;
                }
                UserManager.Set(User);
            }

            FileSyncQueue.Enqueue(new SyncQueueItem()
            {
                CloudFile = newFile,
                OperationData = null,
                RequestedOperation = SyncQueueItem.SyncOperation.Save
            });

            return newFile;
        }

        void CloudFileSearch_FileMatch(CloudFile cacheFile, CloudFile fileSystemFile)
        {
            CoreApp.TraceWriter.Trace("FileMatched: {0}", cacheFile.LocalPath);
            if (cacheFile == null || fileSystemFile == null)
                return;

            lock (CloudFileLock)
            {
                if (FileComparison.Changed(cacheFile, fileSystemFile))
                {
                    cacheFile.LocalDateCreated = fileSystemFile.LocalDateCreated;
                    cacheFile.LocalDateUpdated = fileSystemFile.LocalDateUpdated;

                    UserManager.Set(User);

                    FileSyncQueue.Enqueue(new SyncQueueItem()
                    {
                        CloudFile = cacheFile,
                        OperationData = null,
                        RequestedOperation = SyncQueueItem.SyncOperation.Save
                    });
                }
            }
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
                lock (CloudFileLock)
                {
                    var cloudFile = CloudSearch.FindFile(User.Files, oldFilename);
                    if (cloudFile != null)
                    {
                        cloudFile.LocalPath = newFilename;
                        cloudFile.Name = newFileNoPath;
                        UserManager.Set(User);

                        FileSyncQueue.Enqueue(new SyncQueueItem()
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
            RemoveCloudFile(fileType, fileName);
        }
    }
}
