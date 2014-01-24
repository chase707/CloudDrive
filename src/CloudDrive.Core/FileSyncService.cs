using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using CloudDrive.Service;
using CloudDrive.Data;

namespace CloudDrive.Core
{
	public class FileSyncService
	{
		ICloudService CloudService { get; set; }
		CloudUser CurrentUser { get; set; }
		List<FileSystemWatcher> FolderWatchers { get; set; }

		public FileSyncService(ICloudService cloudService, CloudUser currentUser)
		{
			CloudService = cloudService;
			CurrentUser = currentUser;
			FolderWatchers = new List<FileSystemWatcher>();
		}

		public void WatchFolder(string folderToWatch)
		{
			var folderWatcher = new FileSystemWatcher(folderToWatch);
			folderWatcher.IncludeSubdirectories = true;
			folderWatcher.Changed += FolderWatcher_FileEventOccurred;
			folderWatcher.Created += FolderWatcher_FileEventOccurred;
			folderWatcher.Deleted += FolderWatcher_FileEventOccurred;
		}

		public void SyncFolder(List<CloudFile> refreshFiles, CloudFile parent = null)
		{
			if (refreshFiles == null || refreshFiles.Count <= 0)
				return;

			foreach (var refreshFile in refreshFiles)
			{
				CloudFile cacheFile = RecursiveFindMatch(this.CurrentUser.Files, refreshFile.LocalPath);
				if (ShouldSyncFile(cacheFile, refreshFile))
				{
					Console.WriteLine("\tSyncing File: " + refreshFile.LocalPath);
					this.CloudService.Set(parent, refreshFile);
				}

				if (refreshFile.FileType == CloudFileType.Folder)
					SyncFolder(refreshFile.Children, refreshFile);
			}
		}

		CloudFile RecursiveFindMatch(List<CloudFile> files, string filePath)
		{
			var foundFile = files.FirstOrDefault(f => f.LocalPath.Equals(filePath.ToLower(), StringComparison.OrdinalIgnoreCase));
			if (foundFile != null)
				return foundFile;

			foreach (var file in files.Where(f => f.FileType == CloudFileType.Folder))
			{
				foundFile = RecursiveFindMatch(file.Children, filePath);
				if (foundFile != null)
					return foundFile;
			}
			
			return null;
		}

		void FolderWatcher_FileEventOccurred(object sender, FileSystemEventArgs e)
		{
		}

		bool ShouldSyncFile(CloudFile cacheFile, CloudFile refreshFile)
		{
			if (cacheFile == null)
				return true;

			if (cacheFile.LocalDateUpdated < refreshFile.LocalDateUpdated)
				return true;

			if (string.IsNullOrEmpty(cacheFile.RemoteId))
				return true;

			var cloudInfo = CloudService.Get(cacheFile.RemoteId);
			if (cloudInfo.RemoteDateUpdated < refreshFile.LocalDateUpdated)
				return true;
			
			return false;
		}
	}
}
