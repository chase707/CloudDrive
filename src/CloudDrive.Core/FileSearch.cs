using System.Collections.Generic;
using System.IO;
using System.Linq;
using CloudDrive.Data;

namespace CloudDrive.Core
{
	public class FileSearch
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

            return CloudFile.Create(new System.IO.FileInfo(fullPath), null);
        }
        
		public CloudFile FindFilesAndFolders(string rootFolder, string fileSearchWildcards = "*.*")
		{
			if (!Directory.Exists(rootFolder))
				return null;

			var rootFolderInfo = new System.IO.DirectoryInfo(rootFolder);
            var topLevelFolder = CloudFile.Create(rootFolderInfo, null);
			
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
                var thisChild = CloudFile.Create(dirInfo, parentCloudFolder);

				parentCloudFolder.Children.Add(thisChild);

				// recursively find files and folders from this sub-folder
				RecursiveFileSearch(thisChild, searchString);
			}
		}

		IEnumerable<CloudFile> FindFiles(System.IO.DirectoryInfo parentFolderInfo, CloudFile parentCloudFolder, string searchString)
		{
            return parentFolderInfo.GetFiles(searchString).Select(fi => CloudFile.Create(fi, parentCloudFolder));
		}	
	}
}
