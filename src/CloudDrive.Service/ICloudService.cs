using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CloudDrive.Data;

namespace CloudDrive.Service
{
	public interface ICloudService
	{
		IEnumerable<CloudFile> GetContents(string remotePathOrId);
		CloudFile Get(string remotePathOrId);
		void Set(CloudFile parentFile, CloudFile cloudFile);
	}
}
