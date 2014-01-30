using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CloudDrive.Data;
using CloudDrive.Service;

namespace CloudDrive.Core
{
	public delegate CacheFileManager CacheFileManagerFactory(CloudUser currentUser);
	public class CacheFileManager
	{
		CloudUser CurrentUser { get; set; }
		ICloudFileComparison FileComparison { get; set; }

		public CacheFileManager(CloudUser currentUser, ICloudFileComparison fileComparison)
		{
			CurrentUser = currentUser;
			FileComparison = fileComparison;
		}

		public void EvalDifferences(List<CloudFile> refreshFiles)
		{
			if (refreshFiles == null || refreshFiles.Count <= 0)
				return;

			foreach (var refreshFile in refreshFiles)
			{
				// find a matching file in the cache
				CloudFile cacheFile = RecursiveFindMatch(this.CurrentUser.Files, refreshFile.LocalPath);

				refreshFile.NewOrChanged = FileComparison.IsDifferent(cacheFile, refreshFile);

				// recursively find differences
				if (refreshFile.FileType == CloudFileType.Folder)
					EvalDifferences(refreshFile.Children);
			}
		}

		CloudFile RecursiveFindMatch(List<CloudFile> files, string filePath)
		{
			// TODO: this is a clunky. There should be a better way to find matches,
			// perhaps by flattening the data structure
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
	}
}
