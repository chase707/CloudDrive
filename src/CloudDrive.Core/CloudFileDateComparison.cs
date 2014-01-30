using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudDrive.Data;
using CloudDrive.Service;

namespace CloudDrive.Core
{
	public class CloudFileDateComparison : ICloudFileComparison
	{
		ICloudService CloudService { get; set; }
		public CloudFileDateComparison(ICloudService cloudService)
		{
			this.CloudService = cloudService;
		}

		public bool IsDifferent(CloudFile cachedFile, CloudFile refreshedFile)
		{
			if (cachedFile == null)
				return true;

			if (cachedFile.LocalDateUpdated < refreshedFile.LocalDateUpdated)
				return true;

			if (string.IsNullOrEmpty(cachedFile.RemoteId))
				return true;

			var cloudInfo = CloudService.Get(cachedFile.RemoteId);
			if (cloudInfo.RemoteDateUpdated < refreshedFile.LocalDateUpdated)
				return true;

			return false;
		}
	}
}
