using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using CloudDrive.Service;
using CloudDrive.Data;

namespace CloudDrive.Core
{
	public class SyncQueue
	{
        BlockingCollection<CloudFile> CloudFileQueue { get; set; }
        static object QueueLock = new object();

        public SyncQueue()
        {
            CloudFileQueue = new BlockingCollection<CloudFile>();
        }

        public void Enqueue(CloudFile file)
        {
            lock (QueueLock)
            {
                CloudFileQueue.Add(file);
            }
        }
        
        public CloudFile Dequeue()
        {
            lock (QueueLock)
            {
                return CloudFileQueue.Take();
            }
        }
	}
}
