using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudDrive.Data;

namespace CloudDrive.Core
{
<<<<<<< HEAD
<<<<<<< HEAD
    public delegate CloudUser CloudUserFactory(string userName);
=======
>>>>>>> Refactored FileSync into CacheFileManager and Comparison, external sync
=======
>>>>>>> f8d26e4d8c6b8cdb3423c6b36280233a24eb9515
	public class CloudUserManager
	{
		public ICloudUserDataSource DataSource { get; set; }

		public CloudUserManager(ICloudUserDataSource cloudUserDataSource)
		{
			DataSource = cloudUserDataSource;
		}

		public CloudUser Get(string email)
		{
			var myCloudUser = DataSource.Get(email);
			if (myCloudUser == null)
			{
				myCloudUser = new CloudUser(email);
				
				Set(myCloudUser);
			}

			return myCloudUser;
		}
<<<<<<< HEAD
<<<<<<< HEAD
        
=======

>>>>>>> Refactored FileSync into CacheFileManager and Comparison, external sync
=======

>>>>>>> f8d26e4d8c6b8cdb3423c6b36280233a24eb9515
		public void Set(CloudUser myUser)
		{
			DataSource.Set(myUser);
		}		
	}
}
