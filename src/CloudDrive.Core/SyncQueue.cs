using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using CloudDrive.Service;
using CloudDrive.Data;
using CloudDrive.Tracing;

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
        
        public SyncQueue()
        {
            CloudFileQueue = new BlockingCollection<SyncQueueItem>();
        }

        public void Enqueue(SyncQueueItem file)
        {
            var foundQueue = CloudFileQueue.FirstOrDefault(x => x.CloudFile.LocalPath == file.CloudFile.LocalPath &&
                x.RequestedOperation == file.RequestedOperation);
            if (foundQueue == null)
            {
                CoreApp.TraceWriter.Trace("Enqueueing file: {0}", file.CloudFile.LocalPath);
                CloudFileQueue.Add(file);
            }
            else
            {
                CoreApp.TraceWriter.Trace("File already queued: {0}", file.CloudFile.LocalPath);
            }
        }

        public SyncQueueItem Dequeue()
        {
            return CloudFileQueue.Take();
        }
	}
}
