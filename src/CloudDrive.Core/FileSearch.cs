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
        
        //public CloudFile FindFolder(string fullPath)
        //{
        //    if (!Directory.Exists(fullPath))
        //        return null;

        //    var fi = new System.IO.DirectoryInfo(fullPath);
        //    return new CloudFile()
        //    {
        //        Children = null,
        //        LocalDateCreated = fi.CreationTime,
        //        LocalDateUpdated = fi.LastWriteTime,
        //        LocalPath = fi.FullName,
        //        Name = fi.Name,
        //        FileType = CloudFileType.Folder
        //    };
        //}
        
		public CloudFile FindFilesAndFolders(string rootFolder, string fileSearchWildcards = "*.*")
		{
			if (!Directory.Exists(rootFolder))
				return null;

			var rootFolderInfo = new System.IO.DirectoryInfo(rootFolder);
			var topLevelFolder = CreateCloudFile(rootFolderInfo, null);
			
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
                var thisChild = CreateCloudFile(dirInfo, parentCloudFolder);

				parentCloudFolder.Children.Add(thisChild);

				// recursively find files and folders from this sub-folder
				RecursiveFileSearch(thisChild, searchString);
			}
		}

		IEnumerable<CloudFile> FindFiles(System.IO.DirectoryInfo parentFolderInfo, CloudFile parentCloudFolder, string searchString)
		{
            return parentFolderInfo.GetFiles(searchString).Select(fi => CreateCloudFile(fi, parentCloudFolder));
		}
		
		CloudFile CreateCloudFile(System.IO.FileSystemInfo fsi, CloudFile parent)
		{
			bool folder = fsi.GetType() == typeof(System.IO.DirectoryInfo);
			return new CloudFile()
			{
				Children = folder ? new List<CloudFile>() : null,
				LocalDateCreated = fsi.CreationTime,
				LocalDateUpdated = fsi.LastWriteTime,
				LocalPath = fsi.FullName,
				Name = fsi.Name,
                Parent = parent,
				FileType = folder ? CloudFileType.Folder : CloudFileType.File
			};
		}
	}
}
