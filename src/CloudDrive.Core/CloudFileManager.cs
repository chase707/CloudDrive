using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CloudDrive.Data;
using CloudDrive.Service;

namespace CloudDrive.Core
{
    public delegate CloudFileManager CloudFileManagerFactory(CloudUser currentUser);
	public class CloudFileManager
	{
        ICloudFileChangeComparer FileComparison { get; set; }

        public CloudFileManager(ICloudFileChangeComparer fileComparison)
		{
			FileComparison = fileComparison;
		}
       
        public CloudFile FindFile(List<CloudFile> rootFiles, string localPath)
        {
            return RecursiveFindMatch(rootFiles, localPath);
        }

        public void FindChanges(List<CloudFile> cachedFiles, List<CloudFile> allFiles)
        {
            RecursiveFindChanges(cachedFiles, allFiles);
        }

        public List<CloudFile> FindAllFiles(List<CloudFile> rootFiles)
        {
            List<CloudFile> cloudFiles = new List<CloudFile>();
            var fileSearch = new FileSearch();            
            foreach (var rootFolder in rootFiles)
            {
                var foundFile = fileSearch.FindFilesAndFolders(rootFolder.LocalPath);
                if (foundFile != null)
                    cloudFiles.Add(foundFile);
            }

            return cloudFiles;
        }
        
        void RecursiveFindChanges(List<CloudFile> cachedFiles, List<CloudFile> allFiles)
        {
            if (allFiles == null)
                return;

            foreach (var localFile in allFiles)
            {
                var cacheFile = FindFile(cachedFiles, localFile.LocalPath);                
                localFile.NewOrChanged = FileComparison.Changed(cacheFile, localFile);
                if (localFile.RemoteId == null && cacheFile != null && cacheFile.RemoteId != null)
                {
                    localFile.RemoteDateCreated = cacheFile.RemoteDateCreated;
                    localFile.RemoteDateUpdated = cacheFile.RemoteDateUpdated;
                    localFile.RemoteId = cacheFile.RemoteId;
                    localFile.RemotePath = cacheFile.RemotePath;
                }

                if (localFile.FileType == CloudFileType.Folder)
                    RecursiveFindChanges(cachedFiles, localFile.Children);
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
