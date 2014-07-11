using System;
using System.Collections.Generic;
using System.Linq;
using CloudDrive.Data;
using SkyNet.Client;
using SkyNet.Model;

namespace CloudDrive.Service.SkyDrive
{
    public class SkyDriveCloudService : ICloudService
    {
        Client SkyDriveClient;
        PersistantAccessToken AccessToken;

        public SkyDriveCloudService(string configurationFolder)
        {
            AccessToken = new PersistantAccessToken(configurationFolder);

            SkyDriveClient = CreateSkyClient();

            if (AccessToken.AccessToken != null)
            {
                var refreshedToken = SkyDriveClient.RefreshAccessToken();
                if (refreshedToken.Access_Token != null)
                    AccessToken.AccessToken = refreshedToken;
            }
        }

        public bool Authorized
        {
            get { return AccessToken != null && AccessToken.AccessToken != null && AccessToken.AccessToken.Access_Token != null && AccessToken.AccessToken.Expires_In > 0; }
        }

        public string GetAuthUrl()
        {
            return SkyDriveClient.GetAuthorizationRequestUrl(new Scope[] { Scope.Basic, Scope.Signin, Scope.SkyDrive, Scope.SkyDriveUpdate, Scope.OfflineAccess });
        }

        public void SetAuthorization(string code)
        {
            var accessToken = SkyDriveClient.GetAccessToken(code);
            if (accessToken == null) throw new Exception("Could not get access token given authorization code.");

            if (accessToken != null)
                AccessToken.AccessToken = accessToken;
        }

        public IEnumerable<CloudFile> GetContents(string remotePathOrId)
        {
            var remoteContents = SkyDriveClient.GetContents(remotePathOrId);
            return remoteContents.Select(f => new CloudFile()
            {
                RemotePath = f.Upload_Location,
                FileType = f.Type == "Folder" ? CloudFileType.Folder : CloudFileType.File,
                RemoteDateUpdated = DateTime.Parse(f.Updated_Time),
                RemoteDateCreated = DateTime.Parse(f.Created_Time),
                Name = f.Name
            });
        }

        public CloudFile Get(string remotePathOrId)
        {
            var remoteFileInfo = SkyDriveClient.Get(remotePathOrId);
            if (remoteFileInfo == null)
                return null;

            if (string.IsNullOrEmpty(remoteFileInfo.Id))
                return null;

            return new CloudFile()
            {
                RemotePath = remoteFileInfo.Upload_Location,
                FileType = remoteFileInfo.Type == "Folder" ? CloudFileType.Folder : CloudFileType.File,
                RemoteDateUpdated = DateTime.Parse(remoteFileInfo.Updated_Time),
                RemoteDateCreated = DateTime.Parse(remoteFileInfo.Created_Time),
                Name = remoteFileInfo.Name
            };
        }

        public CloudFile Set(CloudFile cloudFile)
        {
            string parentId = null;
            if (cloudFile.Parent != null)
                parentId = cloudFile.Parent.RemoteId;

            var remoteParentFolder = SkyDriveClient.Get(parentId ?? string.Empty);
            if (remoteParentFolder == null)
            {
                // TODO: Create remote parent folder if not exists
                return null;
            }

            var existingFile = GetRemoteFileByName(cloudFile.Name, parentId);
            var newRemoteFile = CreateOrUpdateRemoteObject(cloudFile, remoteParentFolder, existingFile);

            if (newRemoteFile != null)
            {
                var retFile = CloudFile.ShallowCopy(cloudFile);

                retFile.RemotePath = newRemoteFile.Upload_Location;
                retFile.RemoteDateCreated = DateTime.Parse(newRemoteFile.Created_Time);
                retFile.RemoteDateUpdated = DateTime.Parse(newRemoteFile.Updated_Time);
                retFile.RemoteId = newRemoteFile.Id;

                return retFile;
            }

            return cloudFile;
        }

        public void Rename(CloudFile cloudFile)
        {
            if (!string.IsNullOrEmpty(cloudFile.RemoteId))
            {
                if (cloudFile.FileType == CloudFileType.File)
                    SkyDriveClient.RenameFile(cloudFile.RemoteId, cloudFile.Name);
                else
                    SkyDriveClient.RenameFolder(cloudFile.RemoteId, cloudFile.Name);
            }
            else
            {
                Set(cloudFile);
            }
        }

        File CreateOrUpdateRemoteObject(CloudFile cloudFile, File remoteParent, File existingFile)
        {
            switch (cloudFile.FileType)
            {
                case CloudFileType.Folder:
                    return FindOrCreateRemoteFolder(cloudFile, remoteParent, existingFile);
                case CloudFileType.File:
                    return CreateOrUpdateRemoteFile(cloudFile, remoteParent, existingFile);
            }

            return null;
        }

        File FindOrCreateRemoteFolder(CloudFile cloudFile, File remoteParentFolder, File existingRemoteFolder)
        {
            // only create the remote file it it doesn't exist
            if (existingRemoteFolder == null)
                return SkyDriveClient.CreateFolder(remoteParentFolder.Id, cloudFile.Name);

            return existingRemoteFolder;
        }

        File CreateOrUpdateRemoteFile(CloudFile cloudFile, File remoteParentFolder, File existingRemoteFile)
        {
            File newRemoteFile = null;

            // make sure the file exists locally before syncing it
            if (System.IO.File.Exists(cloudFile.LocalPath))
            {
                // read the file data
                var bytes = System.IO.File.ReadAllBytes(cloudFile.LocalPath);

                // create it remotely
                newRemoteFile = SkyDriveClient.Write(remoteParentFolder.Id, bytes, cloudFile.Name, "application/octet-stream");

                // refresh the file object
                if (newRemoteFile != null)
                    newRemoteFile = SkyDriveClient.Get(newRemoteFile.Id);
            }

            return newRemoteFile;
        }

        File GetRemoteFileByName(string name, string parentFileId = null)
        {
            var remoteChildObjects = SkyDriveClient.GetContents(string.IsNullOrEmpty(parentFileId) ? string.Empty : parentFileId);
            if (remoteChildObjects != null)
                return remoteChildObjects.FirstOrDefault(f => f.Name == name);

            return null;
        }

        Client CreateSkyClient()
        {
            return new Client(Settings.ClientId, Settings.ClientSecret, Settings.LiveLoginCallbackUrl,
                AccessToken.AccessToken != null ? AccessToken.AccessToken.Access_Token : null,
                AccessToken.AccessToken != null ? AccessToken.AccessToken.Refresh_Token : null);
        }
    }
}

