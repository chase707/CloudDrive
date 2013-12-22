using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace CloudDrive.Service.SkyDrive
{
	public static class Settings
	{
		public static string ClientId
		{
			get
			{
				return ConfigurationManager.AppSettings["CloudDrive.Service.SkyDrive.ApplicationId"];
			}
		}
		public static string ClientSecret
		{
			get
			{
				return ConfigurationManager.AppSettings["CloudDrive.Service.SkyDrive.ApplicationSecret"];
			}
		}
		public static string LiveLoginCallbackUrl
		{
			get
			{
				return ConfigurationManager.AppSettings["CloudDrive.Service.SkyDrive.LiveLoginCallbackUrl"];
			}
		}
		public static string LiveBaseUrl
		{
			get
			{
				return ConfigurationManager.AppSettings["CloudDrive.Service.SkyDrive.LiveBaseUrl"];
			}
		}
	}
}
