using MongoDB.Bson;
using System.Collections.Generic;

namespace CloudDrive.Data
{
	public class CloudUser
	{
		public CloudUser(string uniqueName)
		{
			this.UniqueName = uniqueName;
			this.Files = new List<CloudFile>();
		}

		public string UniqueName { get; set; }
		public List<CloudFile> Files { get; set; }
	}
}
