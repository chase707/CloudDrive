using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudDrive.Service;
using CloudDrive.Data;

namespace CloudDrive.Core
{
	public class FileSyncService
	{
		ICloudService CloudService { get; set; }
		CloudUser CurrentUser { get; set; }
		public FileSyncService(ICloudService cloudService, CloudUser currentUser)
		{
			CloudService = cloudService;
			CurrentUser = currentUser;
		}

		public void SyncFolder(List<CloudFile> refreshFiles, CloudFile parent = null)
		{
			if (refreshFiles == null || refreshFiles.Count <= 0)
				return;

			foreach (var refreshFile in refreshFiles)
			{
				// cache files could be null, in which case we still want to 
				CloudFile cacheFile = this.CurrentUser.Files.FirstOrDefault(f => f.LocalPath.ToLower() == refreshFile.LocalPath.ToLower());
				if (ShouldSyncFile(cacheFile, refreshFile))
				{
					// sync file
					Console.WriteLine("\tSyncing File: " + refreshFile.LocalPath);

					this.CloudService.Set(parent, refreshFile);
				}

				if (refreshFile.FileType == CloudFileType.Folder)
					SyncFolder(refreshFile.Children, refreshFile);
			}
		}

		bool ShouldSyncFile(CloudFile cacheFile, CloudFile refreshFile)
		{
			if (cacheFile == null)
				return true;

			if (cacheFile.LocalDateUpdated < refreshFile.LocalDateUpdated)
				return true;

			if (string.IsNullOrEmpty(cacheFile.RemotePath))
				return true;

			var cloudInfo = CloudService.Get(cacheFile.RemotePath);
			if (cloudInfo.RemoteDateUpdated < refreshFile.LocalDateUpdated)
				return true;
			
			return false;
		}
	}
}
