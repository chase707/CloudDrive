using System.IO;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace CloudDrive.Data
{
	public enum CloudFileType
	{
		File,
		Folder
	}

	public class CloudFile
	{
		public string Name { get; set; }
		public CloudFileType FileType { get; set; }

		public string LocalPath { get; set; }
		public DateTime LocalDateCreated { get; set; }
		public DateTime LocalDateUpdated { get; set; }

		public string RemoteId { get; set; }
		public string RemotePath { get; set; }
		public DateTime RemoteDateUpdated { get; set; }
		public DateTime RemoteDateCreated { get; set; }
        
        public CloudFile Parent { get; set; }

		public List<CloudFile> Children { get; set; }

        [ScriptIgnore]
		public bool NewOrChanged { get; set; }

        public static CloudFile Create(FileSystemInfo fsi, CloudFile parent)
        {
            bool folder = fsi.GetType() == typeof(DirectoryInfo);
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

        public static CloudFile ShallowCopy(CloudFile cloudFile)
        {
            if (cloudFile == null || string.IsNullOrEmpty(cloudFile.LocalPath)) return null;

            var fsi = cloudFile.FileType == CloudFileType.File ? (FileSystemInfo)new FileInfo(cloudFile.LocalPath) : (FileSystemInfo)new DirectoryInfo(cloudFile.LocalPath);
            var newFile = Create(fsi, cloudFile.Parent);

            newFile.NewOrChanged = cloudFile.NewOrChanged;
            newFile.RemoteDateCreated = cloudFile.RemoteDateCreated;
            newFile.RemoteDateUpdated = cloudFile.RemoteDateUpdated;
            newFile.RemoteId = cloudFile.RemoteId;
            newFile.RemotePath = cloudFile.RemotePath;
            
            return newFile;
        }
	}
}



