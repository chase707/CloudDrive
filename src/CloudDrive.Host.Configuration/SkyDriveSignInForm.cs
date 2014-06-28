using System.Windows.Forms;
using CloudDrive.Service.SkyDrive;

namespace CloudDrive.Host.Configuration
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
                var authUrl = CloudService.GetAuthUrl();
                signInBrowser.Navigate(authUrl);
				signInBrowser.DocumentCompleted += signInBrowser_DocumentCompleted;
			}
		}

		protected void signInBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			if (e.Url.Query.Contains("code"))
			{
				var code = System.Web.HttpUtility.ParseQueryString(e.Url.Query)["code"];
                if (!string.IsNullOrEmpty(code))
                    CloudService.SetAuthorization(code);
			}
		}
	}
}
