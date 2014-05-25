using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CloudDrive.Data;

namespace CloudDrive.Core
{
	public class FileSearch
	{
        public CloudFile FindFile(string fullPath)
        {
            if (!File.Exists(fullPath))
                return null;

            var fi = new System.IO.FileInfo(fullPath);
            return new CloudFile()
            {
                Children = null,
                LocalDateCreated = fi.CreationTime,
                LocalDateUpdated = fi.LastWriteTime,
                LocalPath = fi.FullName,
                Name = fi.Name,
                FileType = CloudFileType.File
            };
        }

		public CloudFile FindFilesAndFolders(string rootFolder, string fileSearchWildcards = "*.*")
		{
			if (!Directory.Exists(rootFolder))
				return null;

			var rootFolderInfo = new System.IO.DirectoryInfo(rootFolder);
			var thisFolder = new CloudFile()
			{
				Children = new List<CloudFile>(),
				LocalDateCreated = rootFolderInfo.CreationTime,
				LocalDateUpdated = rootFolderInfo.LastWriteTime,
				LocalPath = rootFolderInfo.FullName,
				Name = rootFolderInfo.Name,
				FileType = CloudFileType.Folder
			};

			RecursiveFileSearch(rootFolderInfo, fileSearchWildcards, thisFolder, thisFolder.Children);

			return thisFolder;
		}

		void RecursiveFileSearch(System.IO.DirectoryInfo root, string searchString, CloudFile parentFolder, List<CloudFile> cloudFiles)
		{
			System.IO.FileInfo[] files = null;
            
			try
			{ files = root.GetFiles(searchString); }
			catch (UnauthorizedAccessException) { }
			catch (System.IO.DirectoryNotFoundException e) { }

			if (files != null)
			{
				foreach (System.IO.FileInfo fi in files)
				{
					parentFolder.Children.Add(new CloudFile()
					{
						Children = null,
						LocalDateCreated = fi.CreationTime,
						LocalDateUpdated = fi.LastWriteTime,
						LocalPath = fi.FullName,
						Name = fi.Name,
						FileType = CloudFileType.File
					});
				}

				var subDirs = root.GetDirectories();
				foreach (System.IO.DirectoryInfo dirInfo in subDirs)
				{
					var thisChild = new CloudFile()
					{
						Children = new List<CloudFile>(),
						LocalDateCreated = dirInfo.CreationTime,
						LocalDateUpdated = dirInfo.LastWriteTime,
						LocalPath = dirInfo.FullName,
						Name = dirInfo.Name,
						FileType = CloudFileType.Folder
					};

					parentFolder.Children.Add(thisChild);

					RecursiveFileSearch(dirInfo, searchString, thisChild, thisChild.Children);
				}
			}
		}
	}
}
