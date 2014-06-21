namespace CloudDrive.Configuration.App
{
	partial class SkyDriveSignInForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			signInBrowser = new System.Windows.Forms.WebBrowser();
			SuspendLayout();
			// 
			// signInBrowser
			// 
			signInBrowser.Location = new System.Drawing.Point(2, 1);
			signInBrowser.MinimumSize = new System.Drawing.Size(20, 20);
			signInBrowser.Name = "signInBrowser";
			signInBrowser.Size = new System.Drawing.Size(865, 559);
			signInBrowser.TabIndex = 0;
			// 
			// MainForm
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(867, 554);
			Controls.Add(signInBrowser);
			Name = "MainForm";
			Text = "SkyDrive Desktop";
			ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.WebBrowser signInBrowser;
	}
}

