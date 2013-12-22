using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkyNet;
using SkyNet.Client;
using CloudDrive.Service.SkyDrive;

namespace CloudDrive.Host.Console
{
	class Program
	{
		static Client SkyDriveClient;
		static PersistantAccessToken AccessToken;

		static void Main(string[] args)
		{
			CreateSkyClient();

			var contents = SkyDriveClient.GetContents(string.Empty);
		}

		static void CreateSkyClient()
		{
			AccessToken = new PersistantAccessToken(ConfigurationManager.AppSettings["CloudDrive.Service.SkyDrive.ConfigurationFolder"]);
			SkyDriveClient = new Client(Settings.ClientId, Settings.ClientSecret, Settings.LiveLoginCallbackUrl,
				AccessToken.AccessToken != null ? AccessToken.AccessToken.Access_Token : null,
				AccessToken.AccessToken != null ? AccessToken.AccessToken.Refresh_Token : null);

			if (AccessToken.AccessToken != null)
			{
				AccessToken.AccessToken = SkyDriveClient.RefreshAccessToken();
			}
		}
	}
}
