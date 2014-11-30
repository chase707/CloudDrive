using System.Collections.Concurrent;
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

		public CloudFileType CloudFileType { get; set; }

		/// <summary>
		/// Matching file found in the cloud during scan
		/// </summary>
		public CloudFile MatchedFile { get; set; }

		public string CloudFilename { get; set; }

		public string OldFilename { get; set; }

		public SyncOperation RequestedOperation { get; set; }

		public override string ToString()
		{
			return string.Format("{0} - {1} - {2}", RequestedOperation, CloudFileType, MatchedFile != null ? MatchedFile.LocalPath : CloudFilename);
		}
	}

	public class FileSyncQueue
	{
		BlockingCollection<SyncQueueItem> cloudFileQueue;

		public FileSyncQueue()
		{
			cloudFileQueue = new BlockingCollection<SyncQueueItem>();
		}

		public void Enqueue(SyncQueueItem file)
		{
			cloudFileQueue.Add(file);			
		}

		public SyncQueueItem Dequeue()
		{
			return cloudFileQueue.Take();
		}

		public void Stop()
		{
			cloudFileQueue.CompleteAdding();
		}
	}
}
