namespace CloudDrive.Configuration.App
{
    partial class MainForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.addFolderBtn = new System.Windows.Forms.Button();
            this.removeFolderBtn = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.saveBtn = new System.Windows.Forms.Button();
            this.foldersListBox = new System.Windows.Forms.ListBox();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.foldersListBox);
            this.groupBox1.Controls.Add(this.addFolderBtn);
            this.groupBox1.Controls.Add(this.removeFolderBtn);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(475, 195);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Sync Folders";
            // 
            // addFolderBtn
            // 
            this.addFolderBtn.Location = new System.Drawing.Point(187, 144);
            this.addFolderBtn.Name = "addFolderBtn";
            this.addFolderBtn.Size = new System.Drawing.Size(136, 23);
            this.addFolderBtn.TabIndex = 1;
            this.addFolderBtn.Text = "Add Folder";
            this.addFolderBtn.UseVisualStyleBackColor = true;
            this.addFolderBtn.Click += new System.EventHandler(this.addFolderBtn_Click);
            // 
            // removeFolderBtn
            // 
            this.removeFolderBtn.Location = new System.Drawing.Point(329, 144);
            this.removeFolderBtn.Name = "removeFolderBtn";
            this.removeFolderBtn.Size = new System.Drawing.Size(126, 23);
            this.removeFolderBtn.TabIndex = 2;
            this.removeFolderBtn.Text = "Remove Folder";
            this.removeFolderBtn.UseVisualStyleBackColor = true;
            this.removeFolderBtn.Click += new System.EventHandler(this.removeFolderBtn_Click);
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.Description = "Select folder to sync";
            this.folderBrowserDialog1.RootFolder = System.Environment.SpecialFolder.DesktopDirectory;
            // 
            // saveBtn
            // 
            this.saveBtn.Location = new System.Drawing.Point(251, 216);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(154, 23);
            this.saveBtn.TabIndex = 2;
            this.saveBtn.Text = "Save and Quit";
            this.saveBtn.UseVisualStyleBackColor = true;
            this.saveBtn.Click += new System.EventHandler(this.quitBtn_Click);
            // 
            // foldersListBox
            // 
            this.foldersListBox.FormattingEnabled = true;
            this.foldersListBox.Location = new System.Drawing.Point(7, 20);
            this.foldersListBox.Name = "foldersListBox";
            this.foldersListBox.Size = new System.Drawing.Size(448, 108);
            this.foldersListBox.TabIndex = 3;
            // 
            // cancelBtn
            // 
            this.cancelBtn.Location = new System.Drawing.Point(411, 216);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(75, 23);
            this.cancelBtn.TabIndex = 3;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(498, 251);
            this.ControlBox = false;
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.saveBtn);
            this.Name = "MainForm";
            this.Text = "CloudDrive Configuration";
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button removeFolderBtn;
        private System.Windows.Forms.Button addFolderBtn;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button saveBtn;
        private System.Windows.Forms.ListBox foldersListBox;
        private System.Windows.Forms.Button cancelBtn;
    }
}

