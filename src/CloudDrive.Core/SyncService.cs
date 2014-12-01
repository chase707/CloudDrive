using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CloudDrive.Data;
using CloudDrive.Service;

namespace CloudDrive.Core
{
	public delegate SyncService SyncServiceFactory(CloudUser cloudUser);

	public class SyncService
	{
		CloudUser User { get; set; }
		CloudUserManager UserManager { get; set; }
		FileSyncQueue FileSyncQueue { get; set; }
		FileSystemSearch FileSearch { get; set; }
		FolderWatcher FileWatcher { get; set; }
		ICloudService CloudService { get; set; }
		CloudFileSearch CloudSearch { get; set; }
		ICloudFileChangeComparer FileComparison { get; set; }
		TaskFactory TaskFactory { get; set; }
		bool ServiceRunning { get; set; }
		List<Thread> Threads { get; set; }

		public SyncService(FileSyncQueue syncQueue, FolderWatcher folderWatcher, FileSystemSearch fileSearch,
			CloudUserManager cloudUserManager, CloudFileSearch cloudSearch,
			ICloudService cloudService, ICloudFileChangeComparer fileComparison)
		{
			UserManager = cloudUserManager;
			CloudSearch = cloudSearch;
			CloudService = cloudService;
			FileWatcher = folderWatcher;
			FileSearch = fileSearch;
			FileComparison = fileComparison;
			FileSyncQueue = syncQueue;

			FileWatcher.FileChanged += FileWatcher_FileChanged;
			FileWatcher.FileCreated += FileWatcher_FileCreated;
			FileWatcher.FileDeleted += FileWatcher_FileDeleted;
			FileWatcher.FileRenamed += FileWatcher_FileRenamed;

			CloudSearch.FileMatched += CloudFileSearch_FileMatch;
			CloudSearch.NewFile += CloudFileSearch_NewFile;
			CloudSearch.DeletedFile += CloudFileSearch_FileDeleted;

			User = UserManager.Load();
		}

		public void Start()
		{
			CoreApp.TraceWriter.Trace("Starting Folder Watcher...");

			foreach (var file in User.Files)
			{
				FileWatcher.WatchFolder(file.LocalPath);
			}

			CoreApp.TraceWriter.Trace("Finding Changes Since Last Run...");

			CloudSearch.MatchFiles(User.Files);

			CoreApp.TraceWriter.Trace("Starting Sync Queue...");

			ServiceRunning = true;

			TaskFactory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(System.Environment.ProcessorCount));

			new Thread(new System.Threading.ThreadStart(SyncFileThread)).Start();
				
			CoreApp.TraceWriter.Trace("Sync Queue Started.");
		}

		public void Stop()
		{
			ServiceRunning = false;
			FileSyncQueue.Stop();
		}

		void SyncFileThread()
		{
			while (ServiceRunning)
			{
				try
				{

					var syncItem = FileSyncQueue.Dequeue();					
					if (syncItem != null)
					{
						TaskFactory.StartNew(() => SyncItem(syncItem));
					}
				}
				catch (Exception ex)
				{
					CoreApp.TraceWriter.Trace("SyncQueue Exception: {0}", ex.ToString());
				}
			}
		}

		void SyncItem(SyncQueueItem syncItem)
		{
			if (syncItem != null)
			{
				CoreApp.TraceWriter.Trace("Sync Item Dequeued: {0}", syncItem);
				switch (syncItem.RequestedOperation)
				{
					case SyncQueueItem.SyncOperation.Save:
						{
							if (!SyncSave(syncItem))
							{
								// requeue if failed
								FileSyncQueue.Enqueue(syncItem);
							}
						}
						break;
					case SyncQueueItem.SyncOperation.Rename:
						{
							SyncRename(syncItem);
						}
						break;
					case SyncQueueItem.SyncOperation.Delete:
						{
							SyncDelete(syncItem.CloudFileType, syncItem.CloudFilename);
						}
						break;
					case SyncQueueItem.SyncOperation.None:
						break;
				}
			}
		}

		bool SyncSave(SyncQueueItem syncItem)
		{
			CoreApp.TraceWriter.Trace(string.Format("Syncing File: {0}", syncItem));

			CloudFile cloudFile = null;

			if (syncItem.MatchedFile != null)
			{
				cloudFile = syncItem.MatchedFile;				
			}
			else
			{
				cloudFile = FindFileFromCacheOrFileSystem(syncItem.CloudFileType, syncItem.CloudFilename);
			}

			// ignore files that have been deleted
			if (cloudFile.FileType == CloudFileType.File && !File.Exists(cloudFile.LocalPath))
			{
				return true;
			}

			if (cloudFile.FileType == CloudFileType.Folder && !Directory.Exists(cloudFile.LocalPath))
			{
				return true;
			}
			
			// wait for parent to complete before attempting to upload
			if (cloudFile.Parent != null && string.IsNullOrEmpty(cloudFile.Parent.RemoteId))
			{
				return false;
			}

			if (cloudFile.Parent != null && !string.IsNullOrEmpty(cloudFile.Parent.RemoteId))
			{
				var parentCloudFile = CloudService.Get(cloudFile.Parent.RemoteId);
				if (parentCloudFile == null)
				{
					return false;
				}
			}
				
			var remoteFile = CloudService.Set(cloudFile);
			if (remoteFile == null)
			{
				return false;
			}

			// updated file parents in file tree
			if (cloudFile.Parent != null)
			{
				remoteFile.Parent = cloudFile.Parent;
				remoteFile.Parent.Children.Add(remoteFile);
				remoteFile.Parent.Children.Remove(cloudFile);
				cloudFile.Parent = null;
			}
			else // replace root item
			{
				User.Files.Remove(cloudFile);
				User.Files.Add(remoteFile);
			}

			if (cloudFile.Children != null)
			{
				foreach (var child in cloudFile.Children)
				{
					child.Parent = remoteFile;
					remoteFile.Children.Add(child);
				}

				cloudFile.Children.Clear();
			}

			cloudFile = null;
			syncItem.MatchedFile = null;

			lock (User)
			{
				UserManager.Save(User);
			}

			CoreApp.TraceWriter.Trace(string.Format("Syncing File Complete: {0}", syncItem));

			return true;
		}

		void SyncRename(SyncQueueItem syncItem)
		{
			if (string.IsNullOrEmpty(syncItem.OldFilename) || string.IsNullOrEmpty(syncItem.CloudFilename))
				return;

			var newFileNoPath = Path.GetFileName(syncItem.CloudFilename);

			lock (User)
			{
				var cloudFile = CloudSearch.FindFile(User.Files, syncItem.OldFilename);
				if (cloudFile != null)
				{
					cloudFile.LocalPath = syncItem.CloudFilename;
					cloudFile.Name = newFileNoPath;

					UserManager.Save(User);

					CoreApp.TraceWriter.Trace(string.Format("Renaming File: {0}", cloudFile.LocalPath));
					CloudService.Rename(cloudFile);
					CoreApp.TraceWriter.Trace(string.Format("Renaming File Complete: {0}", cloudFile.LocalPath));
				}
			}
		}

		void SyncDelete(CloudFileType fileType, string fileName)
		{
			// remove from cache
			lock (User)
			{
				var cloudFile = CloudSearch.FindFile(User.Files, fileName);
				if (cloudFile == null) return;

				if (cloudFile.Parent != null)
				{
					cloudFile.Parent.Children.Remove(cloudFile);
					cloudFile.Parent = null;
				}
				else
				{
					User.Files.Remove(cloudFile);
					if (cloudFile.Children != null)
					{
						cloudFile.Children.Clear();
						cloudFile.Children = null;
					}
				}

				UserManager.Save(User);
			}
		}

		CloudFile FindFileFromCacheOrFileSystem(CloudFileType fileType, string fileName)
		{
			var cloudFile = CloudSearch.FindFile(User.Files, fileName);
			if (cloudFile == null)
			{
				cloudFile = TryAndFindFile(fileType, fileName);
				if (cloudFile == null)
				{
					CoreApp.TraceWriter.Trace("{0} - File not found on filesystem", fileName);
					return null;
				}

				var fileInfo = new System.IO.FileInfo(cloudFile.LocalPath);
				var parent = CloudSearch.FindFile(User.Files, fileInfo.DirectoryName);
				if (parent == null)
				{
					CoreApp.TraceWriter.Trace("{0} - Parent not found", fileName);
					return null;
				}

				parent.Children.Add(cloudFile);
				cloudFile.Parent = parent;
			}

			return cloudFile;
		}

		CloudFile TryAndFindFile(CloudFileType fileType, string fileName)
		{
			CoreApp.TraceWriter.Trace("Trying to Find File/Folder on FileSystem {0}", fileName);
			const int maxTrys = 20;
			const int delay = 100;
			var trys = 0;
			// loop until the file is found -
			// a new file/folder could be renamed before it gets added to the cloud file manager
			CloudFile cloudFile = null;
			while (cloudFile == null && trys < maxTrys)
			{
				cloudFile = fileType == CloudFileType.File ? FileSearch.FindFile(fileName) : FileSearch.FindFolder(fileName);
				if (cloudFile != null)
				{
					CoreApp.TraceWriter.Trace("File/Folder Found on FileSystem : {0}", fileName);

					return cloudFile;
				}
				else
				{
					System.Threading.Thread.Sleep(delay);
					trys++;
				}
			}

			return null;
		}

		void CloudFileSearch_FileDeleted(CloudFile deletedFile)
		{
			CoreApp.TraceWriter.Trace("CloudFileSearch_FileDeleted: {0}", deletedFile.LocalPath);

			SyncDelete(deletedFile.FileType, deletedFile.LocalPath);
		}

		CloudFile CloudFileSearch_NewFile(CloudFile parentFolder, CloudFile newFile)
		{
			CoreApp.TraceWriter.Trace("CloudFileSearch_NewFile: {0}, Parent: {1}", newFile.LocalPath, parentFolder.LocalPath);

			// Update parent relationships

			if (parentFolder != null)
			{
				parentFolder.Children.Add(newFile);
				newFile.Parent = parentFolder;
			}
			UserManager.Save(User);

			FileSyncQueue.Enqueue(new SyncQueueItem()
			{
				MatchedFile = newFile,
				RequestedOperation = SyncQueueItem.SyncOperation.Save
			});

			return newFile;
		}

		void CloudFileSearch_FileMatch(CloudFile cacheFile, CloudFile fileSystemFile)
		{
			CoreApp.TraceWriter.Trace("CloudFileSearch_FileMatch: {0}", cacheFile.LocalPath);
			if (cacheFile == null || fileSystemFile == null)
				return;

			if (FileComparison.Changed(cacheFile, fileSystemFile))
			{
				cacheFile.LocalDateCreated = fileSystemFile.LocalDateCreated;
				cacheFile.LocalDateUpdated = fileSystemFile.LocalDateUpdated;

				UserManager.Save(User);

				FileSyncQueue.Enqueue(new SyncQueueItem()
				{
					MatchedFile = cacheFile,
					RequestedOperation = SyncQueueItem.SyncOperation.Save
				});
			}
		}

		void FileWatcher_FileCreated(CloudFileType fileType, string fileName)
		{
			try
			{
				FileSyncQueue.Enqueue(new SyncQueueItem()
				{
					CloudFileType = fileType,
					CloudFilename = fileName,
					RequestedOperation = SyncQueueItem.SyncOperation.Save
				});
			}
			catch (Exception ex)
			{
				CoreApp.TraceWriter.Trace(string.Format("Exception: {0}", ex.ToString()));
			}
		}

		void FileWatcher_FileChanged(CloudFileType fileType, string fileName)
		{
			try
			{
				FileSyncQueue.Enqueue(new SyncQueueItem()
				{
					CloudFileType = fileType,
					CloudFilename = fileName,
					RequestedOperation = SyncQueueItem.SyncOperation.Save
				});
			}
			catch (Exception ex)
			{
				CoreApp.TraceWriter.Trace(string.Format("Exception: {0}", ex.ToString()));
			}
		}

		void FileWatcher_FileRenamed(CloudFileType fileType, string oldFilename, string newFilename)
		{
			try
			{
				FileSyncQueue.Enqueue(new SyncQueueItem()
				{
					CloudFileType = fileType,
					CloudFilename = newFilename,
					OldFilename = oldFilename,
					RequestedOperation = SyncQueueItem.SyncOperation.Rename
				});
			}
			catch (Exception ex)
			{
				CoreApp.TraceWriter.Trace(string.Format("Exception: {0}", ex.ToString()));
			}
		}

		void FileWatcher_FileDeleted(CloudFileType fileType, string fileName)
		{
			FileSyncQueue.Enqueue(new SyncQueueItem()
			{
				CloudFileType = fileType,
				CloudFilename = fileName,
				RequestedOperation = SyncQueueItem.SyncOperation.Delete
			});
		}
	}
}
