using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudDrive.Data;

namespace CloudDrive.Core
{
	public class CloudUserManager
	{
		public ICloudUserDataSource DataSource { get; set; }

		public CloudUserManager(ICloudUserDataSource cloudUserDataSource)
		{
			DataSource = cloudUserDataSource;
		}

		public CloudUser Load()
		{
			var cloudUser = DataSource.Get();
			if (cloudUser == null)
			{
                cloudUser = new CloudUser(CloudUser.GenerateRandomName());
				
				Save(cloudUser);
			}

			return cloudUser;
		}

		public void Save(CloudUser cloudUser)
		{
			DataSource.Set(cloudUser);
		}
	}
}
