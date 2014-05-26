using System.Collections.Generic;
using CloudDrive.Data;

namespace CloudDrive.Service
{
	public interface ICloudService
	{
		IEnumerable<CloudFile> GetContents(string remotePathOrId);
		CloudFile Get(string remotePathOrId);
		void Set(CloudFile cloudFile);
	}
}
