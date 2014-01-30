using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudDrive.Data;

namespace CloudDrive.Core
{
	public interface ICloudFileComparison
	{
		bool IsDifferent(CloudFile localFile, CloudFile remoteFile);
	}
}
