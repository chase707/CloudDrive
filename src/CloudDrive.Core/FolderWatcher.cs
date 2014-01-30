using System;
using System.Collections.Generic;
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
}
