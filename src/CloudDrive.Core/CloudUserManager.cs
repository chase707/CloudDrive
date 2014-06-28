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
			var cloudUser = DataSource.Get();
			if (cloudUser == null)
			{
                cloudUser = new CloudUser(CloudUser.GenerateRandomName());
				
				Set(cloudUser);
			}

            SetParents(cloudUser);

			return cloudUser;
		}

		public void Set(CloudUser cloudUser)
		{
			DataSource.Set(cloudUser);
		}

        private void SetParents(CloudUser cloudUser)
        {
            foreach (var file in cloudUser.Files)
            {
                RecursiveSetParent(file);
            }
        }

        private void RecursiveSetParent(CloudFile cloudFile)
        {
            if (cloudFile.Children == null || cloudFile.Children.Count <= 0)
                return;
            
            foreach (var file in cloudFile.Children)
            {
                file.Parent = cloudFile;

                RecursiveSetParent(file);
            }
        }
	}
}
