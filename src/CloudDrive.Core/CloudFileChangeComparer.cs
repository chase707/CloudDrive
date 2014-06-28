using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudDrive.Data;
using CloudDrive.Service;

namespace CloudDrive.Core
{
    public interface ICloudFileChangeComparer
    {
        bool Changed(CloudFile cacheFile, CloudFile localFile);
     }

    public class CloudFileChangeComparer : ICloudFileChangeComparer
    {
        ICloudService CloudService { get; set; }

        public CloudFileChangeComparer(ICloudService cloudService)
        {
            CloudService = cloudService;
        }

        public bool Changed(CloudFile cacheFile, CloudFile localFile)
        {
            if (cacheFile == null)
                return true;

            if (cacheFile.LocalDateUpdated < localFile.LocalDateUpdated)
                return true;

            if (string.IsNullOrEmpty(cacheFile.RemoteId))
                return true;

            var cloudInfo = CloudService.Get(cacheFile.RemoteId);
            if (cloudInfo == null || cloudInfo.RemoteDateUpdated < localFile.LocalDateUpdated)
                return true;

            return false;
        }
    }
}
