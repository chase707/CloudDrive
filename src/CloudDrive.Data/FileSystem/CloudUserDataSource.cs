using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CloudDrive.Data.FileSystem
{
	public class CloudUserDataSource : ICloudUserDataSource
	{
		protected string StoragePath { get; set; }
		protected string FullFilePath { get; set; }
		const string FileName = "cloudUser.json";

		public CloudUserDataSource(string storagePath)
		{
			this.StoragePath = storagePath;
			this.FullFilePath = Path.Combine(storagePath, FileName);
		}

		public CloudUser Get(string userId)
		{
			if (!File.Exists(FullFilePath))
				return null;

			using (FileStream fs = System.IO.File.OpenRead(FullFilePath))
			{
				using (StreamReader sw = new StreamReader(fs))
				{
					using (JsonReader jr = new JsonTextReader(sw))
					{
						return new JsonSerializer().Deserialize<CloudUser>(jr);
					}
				}
			}		
		}

		public void Set(CloudUser cloudUser)
		{			
			using (FileStream fs = System.IO.File.Open(FullFilePath, FileMode.OpenOrCreate))
			{
				using (StreamWriter sw = new StreamWriter(fs))
				{
					using (JsonWriter jw = new JsonTextWriter(sw))
					{
						jw.Formatting = Formatting.Indented;
						new JsonSerializer().Serialize(jw, cloudUser);
					}
				}
			}
		}
	}
}
