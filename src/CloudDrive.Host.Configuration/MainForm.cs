using System;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;
using CloudDrive.Core;
using CloudDrive.Data;
using CloudDrive.Data.FileSystem;

namespace CloudDrive.Host.Configuration
{
    public partial class MainForm : Form
    {
        CloudUser CloudUser;
        CloudUserManager CloudUserManager;

        public MainForm()
        {
            var cloudUserDataSource = new CloudUserDataSource(ConfigurationManager.AppSettings["CloudDrive.Core.ConfigurationFolder"]);
            CloudUserManager = new CloudUserManager(cloudUserDataSource);
            CloudUser = CloudUserManager.Load();

            InitializeComponent();

            DataBind();
        }

        private void DataBind()
        {
            DataBindFolders();
        }

        private void DataBindFolders()
        {
			foldersListBox.Items.Clear();
            foldersListBox.Items.AddRange(CloudUser.Files.OrderBy(x => x.LocalPath).Select(x => x.LocalPath).ToArray());
        }

        private void addFolderBtn_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (!CloudUser.Files.Any(x => x.LocalPath.Equals(folderBrowserDialog1.SelectedPath, StringComparison.OrdinalIgnoreCase)))
                {
                    var file = new CloudDrive.Core.FileSystemSearch().FindFolder(folderBrowserDialog1.SelectedPath);
                    if (file != null)
                    {
                        CloudUser.Files.Add(file);
                        foldersListBox.Items.Add(file.LocalPath);
                    }
                }
            }
        }

        private void removeFolderBtn_Click(object sender, EventArgs e)
        {
            if (foldersListBox.SelectedItems.Count <= 0) return;

            var result = MessageBox.Show(string.Format("Are your sure you want to delete this folder: {0}", foldersListBox.SelectedItems[0]));
            if (result == System.Windows.Forms.DialogResult.OK)
            {
				var folders = CloudUser.Files.Where(x => x.LocalPath.Equals(foldersListBox.SelectedItems[0] as string, StringComparison.OrdinalIgnoreCase)).ToList();
                foreach (var folder in folders)
                {
                    CloudUser.Files.Remove(folder);
                }

                DataBindFolders();
            }
        }

        private void quitBtn_Click(object sender, EventArgs e)
        {
            CloudUserManager.Save(CloudUser);
            Close();
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            Close();
        }        
    }
}