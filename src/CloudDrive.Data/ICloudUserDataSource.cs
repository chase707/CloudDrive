using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudDrive.Data
{
	interface ICloudUserDataSource
	{
		CloudUser Get(string userId);
		void Set(CloudUser cloudUser);
	}
}
