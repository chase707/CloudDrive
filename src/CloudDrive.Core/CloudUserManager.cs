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

		public CloudUser Get()
		{
			var myCloudUser = DataSource.Get();
			if (myCloudUser == null)
			{
                myCloudUser = new CloudUser(CloudUser.GenerateRandomName());
				
				Set(myCloudUser);
			}

			return myCloudUser;
		}

		public void Set(CloudUser myUser)
		{
			DataSource.Set(myUser);
		}		
	}
}
