using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CTAInstaller
{
	partial class InstallerForm : Form
	{
		private readonly Installer installer;

		public InstallerForm(Installer installer)
		{
			this.InitializeComponent();

			this.installer = installer;
		}

		private async void InstallerForm_Load(object sender, EventArgs e)
		{
			this.installer.ProgressChanged  += this.OnInstallerProgressChanged;
			this.installer.InstallCompleted += this.OnInstallerCompleted;
			this.installer.InstallCancelled += this.OnInstallerCancelled;

			await this.PerformInstallAsync();
		}

		private void OnInstallerCompleted()
		{
			MessageBox.Show("Installation completed successfully.", "Install Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void OnInstallerCancelled()
		{
			MessageBox.Show("Installation was cancelled.", "Install Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Error);
			this.Close();
		}

		private async Task PerformInstallAsync()
		{
			try
			{
				await this.installer.Install();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error: " + ex.Message, "Error During Install", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			this.Close();
		}

		private void OnInstallerProgressChanged(string state, double progress)
		{
			this.Invoke(new Action(() => {
				this.labelState.Text = state;

				if (progress >= 0)
				{
					this.progressBar.Style = ProgressBarStyle.Continuous;
					this.progressBar.Value = (int) ( progress * 100.0 );
				}
				else
				{
					this.progressBar.Style = ProgressBarStyle.Marquee;
				}
			}));
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			this.labelState.Text      = "Cancelling...";
			this.progressBar.Style    = ProgressBarStyle.Marquee;
			this.buttonCancel.Enabled = false;

			this.installer.Cancel();
		}
	}
}