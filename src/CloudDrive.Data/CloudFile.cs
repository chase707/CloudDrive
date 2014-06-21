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

        public static CloudFile Create(System.IO.FileSystemInfo fsi, CloudFile parent)
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



