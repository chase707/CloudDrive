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
    public class SyncQueueItem
    {
        public enum SyncOperation
        {
            None,
            Save,
            Rename,
            Delete
        }

        public CloudFile CloudFile { get; set; }
        public object OperationData { get; set; }
        public SyncOperation RequestedOperation { get; set; }
    }

	public class SyncQueue
	{
        BlockingCollection<SyncQueueItem> CloudFileQueue { get; set; }
        static object QueueLock = new object();

        public SyncQueue()
        {
            CloudFileQueue = new BlockingCollection<SyncQueueItem>();
        }

        public void Enqueue(SyncQueueItem file)
        {
            lock (QueueLock)
            {
                CloudFileQueue.Add(file);
            }
        }

        public SyncQueueItem Dequeue()
        {
            lock (QueueLock)
            {
                return CloudFileQueue.Take();
            }
        }
	}
}
