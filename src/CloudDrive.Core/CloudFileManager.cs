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
		CloudUser CloudUser { get; set; }
        ICloudFileChangeComparer FileComparison { get; set; }

        public CloudFileManager(CloudUser currentUser, ICloudFileChangeComparer fileComparison)
		{
			CloudUser = currentUser;
			FileComparison = fileComparison;
		}
       
        public CloudFile FindFile(string localPath)
        {
            return RecursiveFindMatch(CloudUser.Files, localPath);
        }

        public void FindChanges(CloudUser refreshedUser)
        {
            RecursiveFindChanges(refreshedUser.Files);
        }

        public CloudUser RefreshUser()
        {
            var refreshedUser = new CloudUser(CloudUser.UniqueName);

            var fileSearch = new FileSearch();
            foreach (var rootFolder in CloudUser.Files)
            {
                var foundFile = fileSearch.FindFilesAndFolders(rootFolder.LocalPath);
                if (foundFile != null)
                    refreshedUser.Files.Add(foundFile);
            }

            return refreshedUser;
        }
        
        void RecursiveFindChanges(List<CloudFile> files)
        {
            if (files == null)
                return;

            foreach (var localFile in files)
            {
                var cacheFile = FindFile(localFile.LocalPath);                
                localFile.NewOrChanged = FileComparison.Changed(cacheFile, localFile);
                if (localFile.RemoteId == null && cacheFile != null && cacheFile.RemoteId != null)
                {
                    localFile.RemoteDateCreated = cacheFile.RemoteDateCreated;
                    localFile.RemoteDateUpdated = cacheFile.RemoteDateUpdated;
                    localFile.RemoteId = cacheFile.RemoteId;
                    localFile.RemotePath = cacheFile.RemotePath;
                }

                if (localFile.FileType == CloudFileType.Folder)
                    RecursiveFindChanges(localFile.Children);
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
