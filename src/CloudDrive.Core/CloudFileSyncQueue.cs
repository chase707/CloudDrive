using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudDrive.Data;

namespace CloudDrive.Core
{
	public class CloudFileSyncQueue
	{
		Queue<CloudFile> FileQueue { get; set; }
		public CloudFileSyncQueue()
		{
			FileQueue = new Queue<CloudFile>();
		}

		public void Enqueue(CloudFile file)
		{
			FileQueue.Enqueue(file);
		}

		public CloudFile Dequeue()
		{
			return FileQueue.Dequeue();
		}
	}
}
