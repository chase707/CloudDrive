using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SkyNet;
using SkyNet.Client;
using CloudDrive.Service.SkyDrive;
using CloudDrive.Core;
using CloudDrive.Data;
using CloudDrive.Data.FileSystem;
using CloudDrive.Service;

namespace CloudDrive.Host.ConsoleHost
{
	class Program
	{
        static void Main(string[] args)
		{
			var cloudUser = GetCloudUser();
            var cloudService = new SkyDriveCloudService(ConfigurationManager.AppSettings["CloudDrive.Core.ConfigurationFolder"]);
            var fileComparer = new CloudFileChangeComparer(cloudService);
            //ConfigurationManager.AppSettings["CloudDrive.Core.ConfigurationFolder"]
            var syncService = new SyncService(cloudUser, cloudService, syncQueue, fileSearch, fileWatcher);

            System.Threading.Thread.CurrentThread.Join();           
		}		
	}
}
