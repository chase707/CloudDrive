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
			return new CloudFile()
				{
					RemotePath = remoteFileInfo.Upload_Location,
					FileType = remoteFileInfo.Type == "Folder" ? CloudFileType.Folder : CloudFileType.File,
					RemoteDateUpdated = DateTime.Parse(remoteFileInfo.Updated_Time),
					RemoteDateCreated = DateTime.Parse(remoteFileInfo.Created_Time),
					Name = remoteFileInfo.Name
				};
		}

		public void Set(CloudFile cloudFile)
		{
            string parentId = null;
            if (cloudFile.Parent != null)
                parentId = cloudFile.Parent.RemoteId;

			var remoteParentFolder = SkyDriveClient.Get(parentId ?? string.Empty);
			if (remoteParentFolder == null)
			{
				// TODO: Create remote parent folder if not exists
				return;
			}

			File existingFile = GetRemoteFileByName(cloudFile.Name, parentId);
			File newRemoteFile = CreateRemoteObject(cloudFile, remoteParentFolder, existingFile);
			
			if (newRemoteFile != null)
			{
				cloudFile.RemotePath = newRemoteFile.Upload_Location;
				cloudFile.RemoteDateCreated = DateTime.Parse(newRemoteFile.Created_Time);
				cloudFile.RemoteDateUpdated = DateTime.Parse(newRemoteFile.Updated_Time);
				cloudFile.RemoteId = newRemoteFile.Id;
			}
		}

        public void Rename(CloudFile oldFile, string newFilename)
        {
            if (!string.IsNullOrEmpty(oldFile.RemoteId))
            {
                SkyDriveClient.Rename(oldFile.RemoteId, newFilename);
                oldFile.Name = newFilename;
            }
            else
            {
                Set(oldFile);
            }
        }

		File CreateRemoteObject(CloudFile cloudFile, File remoteParent, File existingFile)
		{
			switch (cloudFile.FileType)
			{ 
				case CloudFileType.Folder:
					return CreateRemoteFolder(cloudFile, remoteParent, existingFile);					
				case CloudFileType.File:
					return CreateRemoteFile(cloudFile, remoteParent, existingFile);					
			}

			return null;
		}

		File CreateRemoteFolder(CloudFile cloudFile,  File remoteParentFolder, File existingRemoteFile)
		{			
			// only create the remote file it it doesn't exist
			if (existingRemoteFile == null)
				return SkyDriveClient.CreateFolder(remoteParentFolder.Id, cloudFile.Name);
			
			return existingRemoteFile;
		}

		File CreateRemoteFile(CloudFile cloudFile, File remoteParentFolder, File existingRemoteFile)
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

