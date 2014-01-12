using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudDrive.Data;
using SkyNet;
using SkyNet.Client;
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
				AccessToken.AccessToken = SkyDriveClient.RefreshAccessToken();
			}
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

		public void Set(CloudFile parentFile, CloudFile cloudFile)
		{
			var remoteParentFolder = SkyDriveClient.Get(parentFile == null ? string.Empty : parentFile.RemoteId);
			if (remoteParentFolder == null)
			{
				// TODO: Create remote parent folder if not exists???
				return;
			}
			var remoteChildObjects = SkyDriveClient.GetContents(parentFile == null ? string.Empty : parentFile.RemoteId);

			SkyNet.Model.File existingRemoteFile = remoteChildObjects.FirstOrDefault(f => f.Name == cloudFile.Name);
			SkyNet.Model.File newRemoteFile = null;
			if (cloudFile.FileType == CloudFileType.Folder)
			{
				// if the remote folder already exists, don't bother creating it
				if (existingRemoteFile == null)
					newRemoteFile = SkyDriveClient.CreateFolder(remoteParentFolder.Id, cloudFile.Name);
				else
					newRemoteFile = existingRemoteFile;
			}
			else if (cloudFile.FileType == CloudFileType.File)
			{
				if (System.IO.File.Exists(cloudFile.LocalPath))
				{
					var bytes = System.IO.File.ReadAllBytes(cloudFile.LocalPath);
					newRemoteFile = SkyDriveClient.Write(remoteParentFolder.Id, bytes, cloudFile.Name, "application/octet-stream");
					if (newRemoteFile != null)
						newRemoteFile = SkyDriveClient.Get(newRemoteFile.Id);
				}			
			}

			if (newRemoteFile != null)
			{
				cloudFile.RemotePath = newRemoteFile.Upload_Location;
				cloudFile.RemoteDateCreated = DateTime.Parse(newRemoteFile.Created_Time);
				cloudFile.RemoteDateUpdated = DateTime.Parse(newRemoteFile.Updated_Time);
				cloudFile.RemoteId = newRemoteFile.Id;				
			}
		}

		Client CreateSkyClient()
		{
			return new Client(Settings.ClientId, Settings.ClientSecret, Settings.LiveLoginCallbackUrl,
				AccessToken.AccessToken != null ? AccessToken.AccessToken.Access_Token : null,
				AccessToken.AccessToken != null ? AccessToken.AccessToken.Refresh_Token : null);
		}
	}
}

