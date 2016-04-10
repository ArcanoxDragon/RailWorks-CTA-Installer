namespace CTAInstaller
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
		protected override void Dispose( bool disposing )
		{
			if ( disposing && ( components != null ) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.sLabelRWLoc = new System.Windows.Forms.Label();
			this.txtRWLoc = new System.Windows.Forms.TextBox();
			this.buttonBrowseRWLoc = new System.Windows.Forms.Button();
			this.labelCurrent = new System.Windows.Forms.Label();
			this.sLabelCurrent = new System.Windows.Forms.Label();
			this.loadingVersion = new System.Windows.Forms.ProgressBar();
			this.sLabelLatest = new System.Windows.Forms.Label();
			this.labelLatest = new System.Windows.Forms.Label();
			this.buttonExit = new System.Windows.Forms.Button();
			this.buttonInstall = new System.Windows.Forms.Button();
			this.openRWDialog = new System.Windows.Forms.OpenFileDialog();
			this.SuspendLayout();
			// 
			// sLabelRWLoc
			// 
			this.sLabelRWLoc.AutoSize = true;
			this.sLabelRWLoc.Location = new System.Drawing.Point(12, 17);
			this.sLabelRWLoc.Name = "sLabelRWLoc";
			this.sLabelRWLoc.Size = new System.Drawing.Size(133, 13);
			this.sLabelRWLoc.TabIndex = 0;
			this.sLabelRWLoc.Text = "RailWorks Install Location:";
			// 
			// txtRWLoc
			// 
			this.txtRWLoc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtRWLoc.Location = new System.Drawing.Point(151, 14);
			this.txtRWLoc.Name = "txtRWLoc";
			this.txtRWLoc.Size = new System.Drawing.Size(316, 20);
			this.txtRWLoc.TabIndex = 1;
			// 
			// buttonBrowseRWLoc
			// 
			this.buttonBrowseRWLoc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonBrowseRWLoc.Location = new System.Drawing.Point(473, 12);
			this.buttonBrowseRWLoc.Name = "buttonBrowseRWLoc";
			this.buttonBrowseRWLoc.Size = new System.Drawing.Size(27, 23);
			this.buttonBrowseRWLoc.TabIndex = 2;
			this.buttonBrowseRWLoc.Text = "...";
			this.buttonBrowseRWLoc.UseVisualStyleBackColor = true;
			this.buttonBrowseRWLoc.Click += new System.EventHandler(this.buttonBrowseRWLoc_Click);
			// 
			// labelCurrent
			// 
			this.labelCurrent.AutoSize = true;
			this.labelCurrent.Location = new System.Drawing.Point(148, 43);
			this.labelCurrent.Name = "labelCurrent";
			this.labelCurrent.Size = new System.Drawing.Size(33, 13);
			this.labelCurrent.TabIndex = 3;
			this.labelCurrent.Text = "None";
			// 
			// sLabelCurrent
			// 
			this.sLabelCurrent.AutoSize = true;
			this.sLabelCurrent.Location = new System.Drawing.Point(12, 43);
			this.sLabelCurrent.Name = "sLabelCurrent";
			this.sLabelCurrent.Size = new System.Drawing.Size(122, 13);
			this.sLabelCurrent.TabIndex = 3;
			this.sLabelCurrent.Text = "Current installed version:";
			// 
			// loadingVersion
			// 
			this.loadingVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.loadingVersion.Location = new System.Drawing.Point(151, 66);
			this.loadingVersion.Name = "loadingVersion";
			this.loadingVersion.Size = new System.Drawing.Size(349, 23);
			this.loadingVersion.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
			this.loadingVersion.TabIndex = 4;
			// 
			// sLabelLatest
			// 
			this.sLabelLatest.AutoSize = true;
			this.sLabelLatest.Location = new System.Drawing.Point(12, 72);
			this.sLabelLatest.Name = "sLabelLatest";
			this.sLabelLatest.Size = new System.Drawing.Size(121, 13);
			this.sLabelLatest.TabIndex = 3;
			this.sLabelLatest.Text = "Latest version available:";
			// 
			// labelLatest
			// 
			this.labelLatest.AutoSize = true;
			this.labelLatest.Location = new System.Drawing.Point(148, 72);
			this.labelLatest.Name = "labelLatest";
			this.labelLatest.Size = new System.Drawing.Size(33, 13);
			this.labelLatest.TabIndex = 3;
			this.labelLatest.Text = "None";
			this.labelLatest.Visible = false;
			// 
			// buttonExit
			// 
			this.buttonExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonExit.Location = new System.Drawing.Point(330, 108);
			this.buttonExit.Name = "buttonExit";
			this.buttonExit.Size = new System.Drawing.Size(82, 31);
			this.buttonExit.TabIndex = 2;
			this.buttonExit.Text = "Exit";
			this.buttonExit.UseVisualStyleBackColor = true;
			this.buttonExit.Click += new System.EventHandler(this.buttonExit_Click);
			// 
			// buttonInstall
			// 
			this.buttonInstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonInstall.Enabled = false;
			this.buttonInstall.Location = new System.Drawing.Point(418, 108);
			this.buttonInstall.Name = "buttonInstall";
			this.buttonInstall.Size = new System.Drawing.Size(82, 31);
			this.buttonInstall.TabIndex = 2;
			this.buttonInstall.Text = "Install";
			this.buttonInstall.UseVisualStyleBackColor = true;
			this.buttonInstall.Click += new System.EventHandler(this.buttonInstall_Click);
			// 
			// openRWDialog
			// 
			this.openRWDialog.FileName = "RailWorks.exe";
			this.openRWDialog.Filter = "RailWorks Executable (RailWorks.exe)|RailWorks.exe";
			this.openRWDialog.InitialDirectory = "C:\\Program Files\\";
			this.openRWDialog.Title = "Locate the RailWorks game executable";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(512, 151);
			this.Controls.Add(this.sLabelLatest);
			this.Controls.Add(this.loadingVersion);
			this.Controls.Add(this.sLabelCurrent);
			this.Controls.Add(this.labelLatest);
			this.Controls.Add(this.labelCurrent);
			this.Controls.Add(this.buttonInstall);
			this.Controls.Add(this.buttonExit);
			this.Controls.Add(this.buttonBrowseRWLoc);
			this.Controls.Add(this.txtRWLoc);
			this.Controls.Add(this.sLabelRWLoc);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.Text = "CTA Installer";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label sLabelRWLoc;
		private System.Windows.Forms.TextBox txtRWLoc;
		private System.Windows.Forms.Button buttonBrowseRWLoc;
		private System.Windows.Forms.Label labelCurrent;
		private System.Windows.Forms.Label sLabelCurrent;
		private System.Windows.Forms.ProgressBar loadingVersion;
		private System.Windows.Forms.Label sLabelLatest;
		private System.Windows.Forms.Label labelLatest;
		private System.Windows.Forms.Button buttonExit;
		private System.Windows.Forms.Button buttonInstall;
		private System.Windows.Forms.OpenFileDialog openRWDialog;
	}
}

