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
			this.signInBrowser = new System.Windows.Forms.WebBrowser();
			this.SuspendLayout();
			// 
			// signInBrowser
			// 
			this.signInBrowser.Location = new System.Drawing.Point(2, 1);
			this.signInBrowser.MinimumSize = new System.Drawing.Size(20, 20);
			this.signInBrowser.Name = "signInBrowser";
			this.signInBrowser.Size = new System.Drawing.Size(865, 559);
			this.signInBrowser.TabIndex = 0;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(867, 554);
			this.Controls.Add(this.signInBrowser);
			this.Name = "MainForm";
			this.Text = "SkyDrive Desktop";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.WebBrowser signInBrowser;
	}
}

