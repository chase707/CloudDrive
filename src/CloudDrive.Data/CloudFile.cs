using MongoDB.Bson;
using System;
using System.Collections.Generic;

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

		//public CloudFile ShallowCopy()
		//{
		//	return new CloudFile()
		//	{
		//		Name = this.Name,
		//		Children = new List<CloudFile>(),
		//		LocalDateCreated = this.DateCreated,
		//		LocalDateUpdated = this.DateUpdated,
		//		LocalPath = this.LocalPath,
		//		FileType = this.FileType,
		//		RemotePath = this.RemotePath
		//	};
		//}
	}
}



