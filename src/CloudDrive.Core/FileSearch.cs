using System.Collections.Generic;
using System.IO;
using System.Linq;
using CloudDrive.Data;

namespace CloudDrive.Core
{
	public class FileSearch
	{
		public CloudFile FindFilesAndFolders(string rootFolder, string fileSearchWildcards = "*.*")
		{
			if (!Directory.Exists(rootFolder))
				return null;

			var rootFolderInfo = new System.IO.DirectoryInfo(rootFolder);
			var topLevelFolder = CreateCloudFile(rootFolderInfo);
			
			RecursiveFileSearch(topLevelFolder, fileSearchWildcards);

			return topLevelFolder;
		}

		void RecursiveFileSearch(CloudFile parentCloudFolder, string searchString)
		{
			var parentFolderInfo = new System.IO.DirectoryInfo(parentCloudFolder.LocalPath);

			// get all sub-files in parent folder
			var files = FindFiles(parentFolderInfo, parentCloudFolder, searchString);
			if (files.Count() > 0)
				parentCloudFolder.Children.AddRange(files);

			// get all sub-folders
			foreach (System.IO.DirectoryInfo dirInfo in parentFolderInfo.GetDirectories())
			{
				var thisChild = CreateCloudFile(dirInfo);

				parentCloudFolder.Children.Add(thisChild);

				// recursively find files and folders from this sub-folder
				RecursiveFileSearch(thisChild, searchString);
			}
		}

		IEnumerable<CloudFile> FindFiles(System.IO.DirectoryInfo parentFolderInfo, CloudFile parentCloudFolder, string searchString)
		{
			return parentFolderInfo.GetFiles(searchString).Select(fi => CreateCloudFile(fi));
		}
		
		CloudFile CreateCloudFile(System.IO.FileSystemInfo fsi)
		{
			bool folder = fsi.GetType() == typeof(System.IO.DirectoryInfo);
			return new CloudFile()
			{
				Children = folder ? new List<CloudFile>() : null,
				LocalDateCreated = fsi.CreationTime,
				LocalDateUpdated = fsi.LastWriteTime,
				LocalPath = fsi.FullName,
				Name = fsi.Name,
				FileType = folder ? CloudFileType.Folder : CloudFileType.File
			};
		}
	}
}
