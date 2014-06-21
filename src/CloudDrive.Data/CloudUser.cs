using System.Collections.Generic;
using System.Linq;
using System;

namespace CloudDrive.Data
{
	public class CloudUser
	{
		public CloudUser(string uniqueName)
		{
			UniqueName = uniqueName;
			Files = new List<CloudFile>();
		}

        public static string GenerateRandomName()
        {
            return Guid.NewGuid().ToString();
        }

		public string UniqueName { get; set; }
		public List<CloudFile> Files { get; set; }
	}
}
