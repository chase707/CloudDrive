using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using System.IO;
using CloudDrive.Service.SkyDrive;
using System.Configuration;

namespace CloudDrive.Configuration.App
{
	public partial class SkyDriveSignInForm : Form
	{
        SkyDriveCloudService CloudService;

        public SkyDriveSignInForm(SkyDriveCloudService cloudService)
		{
			InitializeComponent();

            CloudService = cloudService;            
            if (!CloudService.Authorized)
			{
                this.signInBrowser.Navigate(CloudService.GetAuthUrl());
				this.signInBrowser.DocumentCompleted += signInBrowser_DocumentCompleted;
			}
		}

		protected void signInBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			if (e.Url.Query.Contains("code"))
			{
				var code = System.Web.HttpUtility.ParseQueryString(e.Url.Query)["code"];
                if (string.IsNullOrEmpty(code)) 
                    MessageBox.Show("Error logging into one drive.");
                else
                    CloudService.SetAuthorization(code);
			}
		}
	}
}
