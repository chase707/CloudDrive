using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CloudDrive.Data;
using CloudDrive.Service;

namespace CloudDrive.Core
{
    public delegate CloudFileSearch CloudFileManagerFactory(CloudUser currentUser);

    /// <summary>
    /// Searches the CloudFile caches and looks for changes
    /// </summary>
	public class CloudFileSearch
	{
        public delegate void FileMatchFoundHandler(CloudFile cacheFile, CloudFile fileSystemFile);
        public FileMatchFoundHandler FileMatched;

        public delegate CloudFile NewFileFoundHandler(CloudFile parentFile, CloudFile fileSystemFile);
        public NewFileFoundHandler NewFile;

        public delegate void DeletedFileHandler(CloudFile cacheFile);
        public DeletedFileHandler DeletedFile;

        public CloudFile FindFile(List<CloudFile> rootFiles, string localPath)
        {
            return RecursiveFindFile(rootFiles, localPath);
        }

        public void MatchFiles(IEnumerable<CloudFile> cacheFiles)
        {
            var fileSearch = new FileSystemSearch();
            foreach (var cacheFile in cacheFiles)
            {
                var folder = fileSearch.FindFolder(cacheFile.LocalPath);
                if (folder != null)
                {
                    if (FileMatched != null)
                    {
                        FileMatched(cacheFile, folder);
                    }
                }
                else
                {
                    if (DeletedFile != null)
                    {
                        DeletedFile(cacheFile);
                    }
                }
                RecursiveMatchFiles(cacheFile, fileSearch.FindFilesAndFolders(cacheFile.LocalPath));
            }
        }

        void RecursiveMatchFiles(CloudFile parentFile, IEnumerable<CloudFile> fileSystemFiles)
        {
            foreach (var foundFile in fileSystemFiles)
            {
                var matchedFile = MatchFile(parentFile.Children, foundFile.LocalPath);

                FireMatch(parentFile, matchedFile, foundFile);
            }

            foreach (var childFile in parentFile.Children.ToList()) // hack - create a copy so events that modify collection don't break
            {
                var matchedFile = MatchFile(fileSystemFiles, childFile.LocalPath);

                FireDeleted(matchedFile, childFile);
            }

            foreach (var fileSystemFile in fileSystemFiles.Where(x => x.FileType == CloudFileType.Folder))
            {
                var existingParent = MatchFile(parentFile.Children, fileSystemFile.LocalPath);
                if (existingParent == null)
                {
                    existingParent = NewFile(parentFile, fileSystemFile);
                }
                var childFiles = new FileSystemSearch().FindFilesAndFolders(fileSystemFile.LocalPath);
                RecursiveMatchFiles(existingParent, childFiles);
            }
        }

        void FireMatch(CloudFile parentFile, CloudFile matchedFile, CloudFile fileSystemFile)
        {
            if (matchedFile != null)
            {
                if (FileMatched != null)
                {
                    // updated existing listing
                    FileMatched(matchedFile, fileSystemFile);
                }
            }
            else
            {
                if (NewFile != null)
                {
                    // insert new listing
                    NewFile(parentFile, fileSystemFile);
                }
            }
        }

        void FireDeleted(CloudFile matchedFile, CloudFile childFile)
        {
            if (matchedFile == null)
            {
                if (DeletedFile != null)
                {
                    // delete existing listing
                    DeletedFile(childFile);
                }
            }
        }

        CloudFile MatchFile(IEnumerable<CloudFile> files, string filePath)
        {
            if (files == null || files.Count() <= 0) return null;

            return files.FirstOrDefault(f => f.LocalPath.Equals(filePath.ToLower(), StringComparison.OrdinalIgnoreCase));
        }

        CloudFile RecursiveFindFile(List<CloudFile> files, string filePath)
        {
            var foundFile = files.FirstOrDefault(f => f.LocalPath.Equals(filePath.ToLower(), StringComparison.OrdinalIgnoreCase));
            if (foundFile != null)
                return foundFile;

            foreach (var file in files.Where(f => f.FileType == CloudFileType.Folder))
            {
                foundFile = RecursiveFindFile(file.Children, filePath);
                if (foundFile != null)
                    return foundFile;
            }

            return null;
        }
	}
}
