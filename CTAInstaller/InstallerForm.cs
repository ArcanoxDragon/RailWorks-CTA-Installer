using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CTAInstaller
{
	partial class InstallerForm : Form
	{
		private Installer installer;

		public InstallerForm( Installer installer )
		{
			InitializeComponent();

			this.installer = installer;
		}

		private void InstallerForm_Load( object sender, EventArgs e )
		{
			this.installer.ProgressChanged += OnInstallerProgressChanged;
			this.installer.InstallCompleted += OnInstallerCompleted;
			this.installer.InstallCancelled += OnInstallerCancelled;

			this.installer.Install()
				.ContinueWith( this.OnInstallTaskFinish );
		}

		void OnInstallerCompleted()
		{
			MessageBox.Show( "Installation completed successfully.", "Install Complete", MessageBoxButtons.OK, MessageBoxIcon.Information );
		}

		void OnInstallerCancelled()
		{
			MessageBox.Show( "Installation was cancelled.", "Install Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Error );
			this.Close();
		}

		async void OnInstallTaskFinish( Task installTask )
		{
			try
			{
				await installTask;
			}
			catch ( Exception ex )
			{
				MessageBox.Show( "Error: " + ex.Message, "Error During Install", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}

			this.Invoke( new Action( this.Close ) );
		}

		void OnInstallerProgressChanged( string state, double progress )
		{
			this.Invoke( new Action( () =>
			{
				this.labelState.Text = state;

				if ( progress >= 0 )
				{
					this.progressBar.Style = ProgressBarStyle.Continuous;
					this.progressBar.Value = (int) ( progress * 100.0 );
				}
				else
				{
					this.progressBar.Style = ProgressBarStyle.Marquee;
				}
			} ) );
		}

		private void buttonCancel_Click( object sender, EventArgs e )
		{
			Task.Run( () => this.Invoke( new Action( () =>
			{
				this.labelState.Text = "Cancelling...";
				this.progressBar.Style = ProgressBarStyle.Marquee;
				this.buttonCancel.Enabled = false;
			} ) ) );

			this.installer.Cancel();
		}
	}
}
