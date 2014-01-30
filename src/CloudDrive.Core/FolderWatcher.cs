using System;
using System.Collections.Generic;
<<<<<<< HEAD
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace CloudDrive.Core
{
    //public delegate void FolderChangedDelegate(string folderName);
    public delegate void FileChangedDelegate(string fileName);
    public delegate void FileCreatedDelegate(string fileName);
    public delegate void FileDeletedDelegate(string fileName);
    public delegate void FileRenamedDelegate(string oldFilename, string newFilename);

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
                FileWatchers.Add(folderToWatch, CreateWatcher(folderToWatch, NotifyFilters.FileName));
            }

            if (!FolderWatchers.ContainsKey(folderToWatch))
            {
                FolderWatchers.Add(folderToWatch, CreateWatcher(folderToWatch,  NotifyFilters.DirectoryName));
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
            if (FolderWatchers.ContainsValue((FileSystemWatcher)sender))
            {
                Console.WriteLine("Directory renamed:{0}", e.FullPath);
            }
            else
            {
                Console.WriteLine("File renamed:{0}", e.FullPath);
            }

            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Renamed:
                    FireFileRenamed(e);
                    break;
            }
        }

        void FolderWatcher_FileEventOccurred(object sender, FileSystemEventArgs e)
        {
            if (FolderWatchers.ContainsValue((FileSystemWatcher)sender))
            {
                Console.WriteLine("Directory:{0}", e.FullPath);
            }
            else
            {
                Console.WriteLine("File:{0}", e.FullPath);
            }

            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Changed:
                    FireFileChanged(e);
                    break;
                case WatcherChangeTypes.Created:
                    FireFileCreated(e);
                    break;
                case WatcherChangeTypes.Deleted:
                    FireFileDeleted(e);
                    break;
            }
        }

        void FireFileChanged(FileSystemEventArgs e)
        {
            if (FileChanged != null)
                FileChanged(e.FullPath);
        }

        void FireFileCreated(FileSystemEventArgs e)
        {
            if (FileCreated != null)
                FileCreated(e.FullPath);
        }

        void FireFileDeleted(FileSystemEventArgs e)
        {
            if (FileDeleted != null)
                FileDeleted(e.FullPath);
        }

        void FireFileRenamed(RenamedEventArgs e)
        {            
            if (FileRenamed != null)
                FileRenamed(e.OldFullPath, e.FullPath);
        }
    }
=======
using System.IO;
using System.Linq;
using CloudDrive.Data;
using CloudDrive.Service;

namespace CloudDrive.Core
{
	public class FolderWatcher
	{
		List<FileSystemWatcher> FolderWatchers { get; set; }

		public FolderWatcher()
		{
			FolderWatchers = new List<FileSystemWatcher>();
		}

		public void WatchFolder(string folderToWatch)
		{
			var folderWatcher = new FileSystemWatcher(folderToWatch);
			folderWatcher.IncludeSubdirectories = true;
			folderWatcher.Changed += FolderWatcher_FileEventOccurred;
			folderWatcher.Created += FolderWatcher_FileEventOccurred;
			folderWatcher.Deleted += FolderWatcher_FileEventOccurred;

			FolderWatchers.Add(folderWatcher);
		}

		void FolderWatcher_FileEventOccurred(object sender, FileSystemEventArgs e)
		{
			// TODO: Implement file compare and event trigger to sync queue
		}
	}
>>>>>>> Refactored FileSync into CacheFileManager and Comparison, external sync
}
