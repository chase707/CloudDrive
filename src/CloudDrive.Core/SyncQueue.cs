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
	public class SyncQueue
	{
        ICloudFileChangeComparer FileComparer { get; set; }
		CloudUser CurrentUser { get; set; }
        Queue<CloudFile> CloudFileQueue { get; set; }
        static object QueueLock = new object();

        public SyncQueue(CloudUser currentUser, ICloudFileChangeComparer fileComparer)
		{
            FileComparer = fileComparer;
			CurrentUser = currentUser;
		}
        
        public void EnqueueFile(CloudFile file, CloudFile parent = null)
        {
            lock (QueueLock)
            {
                var cacheFile = RecursiveFindMatch(CurrentUser.Files, file.LocalPath);
                if (FileComparer.Changed(cacheFile, file))
                {
                    CloudFileQueue.Enqueue(file);
                }
            }
        }

		public void EnqueueFileTree(CloudFile localFolder, CloudFile parent = null)
		{
            lock (QueueLock)
            {
                EnqueueFile(localFolder, parent);

                if (localFolder.FileType != CloudFileType.Folder)
                    return;

                foreach (var localFile in localFolder.Children)
                {
                    EnqueueFile(localFile, parent);

                    if (localFile.FileType == CloudFileType.Folder)
                        EnqueueFileTree(localFile, localFile);
                }
            }
		}

        public CloudFile Dequeue()
        {
            lock (QueueLock)
            {
                return CloudFileQueue.Dequeue();
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
	}
}
