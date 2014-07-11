using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace CloudDrive.Core
{
    public delegate void FileChangedDelegate(CloudDrive.Data.CloudFileType fileType, string fileName);
    public delegate void FileCreatedDelegate(CloudDrive.Data.CloudFileType fileType, string fileName);
    public delegate void FileDeletedDelegate(CloudDrive.Data.CloudFileType fileType, string fileName);
    public delegate void FileRenamedDelegate(CloudDrive.Data.CloudFileType fileType, string oldFilename, string newFilename);

    public class FolderWatcher
    {
        Dictionary<string, FileSystemWatcher> FolderWatchers { get; set; }
        Dictionary<string, FileSystemWatcher> FileWatchers { get; set; }

        public event FileChangedDelegate FileChanged;
        public event FileCreatedDelegate FileCreated;
        public event FileRenamedDelegate FileRenamed;
        public event FileDeletedDelegate FileDeleted;

        public FolderWatcher()
        {
            FolderWatchers = new Dictionary<string, FileSystemWatcher>();
            FileWatchers = new Dictionary<string, FileSystemWatcher>();
        }

        public void WatchFolder(string folderToWatch)
        {
            if (!Directory.Exists(folderToWatch)) return;

            if (!FileWatchers.ContainsKey(folderToWatch))
            {
                FileWatchers.Add(folderToWatch, CreateWatcher(folderToWatch, NotifyFilters.LastWrite | NotifyFilters.FileName));
            }

            if (!FolderWatchers.ContainsKey(folderToWatch))
            {
                FolderWatchers.Add(folderToWatch, CreateWatcher(folderToWatch, NotifyFilters.DirectoryName));
            }
        }

        FileSystemWatcher CreateWatcher(string folder, NotifyFilters notifyFilters)
        {
            var watcher = new FileSystemWatcher(folder);
            watcher.InternalBufferSize = 16384;
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = notifyFilters;
            watcher.Changed += FolderWatcher_FileEventOccurred;
            watcher.Created += FolderWatcher_FileEventOccurred;
            watcher.Deleted += FolderWatcher_FileEventOccurred;
            watcher.Renamed += FolderWatcher_RenamedEventOccurred;
            watcher.EnableRaisingEvents = true;

            return watcher;
        }

        void FolderWatcher_RenamedEventOccurred(object sender, RenamedEventArgs e)
        {
            var fileType = Data.CloudFileType.File; 
            if (FolderWatchers.ContainsValue((FileSystemWatcher)sender))
            {
                fileType = Data.CloudFileType.Folder;
                CoreApp.TraceWriter.Trace("Directory renamed:{0}", e.FullPath);
            }
            else
            {
                CoreApp.TraceWriter.Trace("File renamed:{0}", e.FullPath);
            }

            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Renamed:
                    FireFileRenamed(fileType, e);
                    break;
            }
        }

        void FolderWatcher_FileEventOccurred(object sender, FileSystemEventArgs e)
        {
            var fileType = Data.CloudFileType.File;
            if (FolderWatchers.ContainsValue((FileSystemWatcher)sender))
            {
                fileType = Data.CloudFileType.Folder;
                CoreApp.TraceWriter.Trace("Directory:{0} {1}", e.FullPath, e.ChangeType);
            }
            else
            {
                CoreApp.TraceWriter.Trace("File:{0} {1}", e.FullPath, e.ChangeType);
            }

            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Changed:
                    FireFileChanged(fileType, e);
                    break;
                case WatcherChangeTypes.Created:
                    FireFileCreated(fileType, e);
                    break;
                case WatcherChangeTypes.Deleted:
                    FireFileDeleted(fileType, e);
                    break;
            }
        }

        void FireFileChanged(CloudDrive.Data.CloudFileType fileType, FileSystemEventArgs e)
        {
            if (FileChanged != null)
                FileChanged(fileType, e.FullPath);
        }

        void FireFileCreated(CloudDrive.Data.CloudFileType fileType, FileSystemEventArgs e)
        {
            if (FileCreated != null)
                FileCreated(fileType, e.FullPath);
        }

        void FireFileDeleted(CloudDrive.Data.CloudFileType fileType, FileSystemEventArgs e)
        {
            if (FileDeleted != null)
                FileDeleted(fileType, e.FullPath);
        }

        void FireFileRenamed(CloudDrive.Data.CloudFileType fileType, RenamedEventArgs e)
        {
            if (FileRenamed != null)
                FileRenamed(fileType, e.OldFullPath, e.FullPath);
        }
    }
}
