using System.Collections.Generic;
using System.IO;
using System.Linq;
using CloudDrive.Data;

namespace CloudDrive.Core
{
    /// <summary>
    /// FileSearch searchs the local file system for files and folders
    /// </summary>
	public class FileSystemSearch
	{
        public CloudFile FindFile(string fullPath)
        {
            if (!File.Exists(fullPath))
                return null;

            return CloudFile.Create(new System.IO.FileInfo(fullPath), null);
        }

        public CloudFile FindFolder(string fullPath)
        {
            if (!Directory.Exists(fullPath))
                return null;

            return CloudFile.Create(new System.IO.DirectoryInfo(fullPath), null);
        }

        public IEnumerable<CloudFile> FindFilesAndFolders(string rootFolder)
        {
            if (!Directory.Exists(rootFolder))
                return null;

            var rootFolderInfo = new System.IO.DirectoryInfo(rootFolder);
            var topLevelFolder = CloudFile.Create(rootFolderInfo, null);

            return FindFolders(rootFolderInfo, topLevelFolder)
                .Union(FindFiles(rootFolderInfo, topLevelFolder));
        }
        
		public CloudFile FindFilesAndFolders(string rootFolder, bool recurse)
		{
			if (!Directory.Exists(rootFolder))
				return null;

			var rootFolderInfo = new System.IO.DirectoryInfo(rootFolder);
            var topLevelFolder = CloudFile.Create(rootFolderInfo, null);

            if (recurse)
            {
                RecursiveFileSearch(topLevelFolder);
            }
            else
            {
                FolderSearch(rootFolderInfo, topLevelFolder);
                FileSearch(rootFolderInfo, topLevelFolder);
            }

			return topLevelFolder;
		}

		void RecursiveFileSearch(CloudFile parentCloudFolder)
		{
			var parentFolderInfo = new System.IO.DirectoryInfo(parentCloudFolder.LocalPath);

            FileSearch(parentFolderInfo, parentCloudFolder);

			// get all sub-folders
			foreach (System.IO.DirectoryInfo dirInfo in parentFolderInfo.GetDirectories())            
            {
                var thisChild = CloudFile.Create(dirInfo, parentCloudFolder);

				parentCloudFolder.Children.Add(thisChild);

				// recursively find files and folders from this sub-folder
				RecursiveFileSearch(thisChild);
			}
		}

        void FileSearch(System.IO.DirectoryInfo parentFolderInfo, CloudFile parentCloudFolder)
        {
            // get all sub-files in parent folder
            var files = FindFiles(parentFolderInfo, parentCloudFolder);
            if (files.Count() > 0)
                parentCloudFolder.Children.AddRange(files);
        }

        void FolderSearch(System.IO.DirectoryInfo parentFolderInfo, CloudFile parentCloudFolder)
        {
            // get all sub-files in parent folder
            var files = FindFolders(parentFolderInfo, parentCloudFolder);
            if (files.Count() > 0)
                parentCloudFolder.Children.AddRange(files);
        }

		IEnumerable<CloudFile> FindFiles(System.IO.DirectoryInfo parentFolderInfo, CloudFile parentCloudFolder)
		{
            return parentFolderInfo.GetFiles().Select(fi => CloudFile.Create(fi, parentCloudFolder));
		}

        IEnumerable<CloudFile> FindFolders(System.IO.DirectoryInfo parentFolderInfo, CloudFile parentCloudFolder)
        {
            return parentFolderInfo.GetDirectories().Select(fi => CloudFile.Create(fi, parentCloudFolder));
        }	
	}
}
