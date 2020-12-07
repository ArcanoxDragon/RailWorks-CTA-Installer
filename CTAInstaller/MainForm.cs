using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using CTAInstaller.Properties;
using CTALib;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace CTAInstaller
{
	public partial class MainForm : Form
	{
		private string            rwLoc;
		private InstallState      installState = InstallState.NotInstalled;
		private VersionInfoLocal  currentVersion;
		private VersionInfoRemote latestVersion;
		private bool              hasLatest, validPath;

		public MainForm()
		{
			this.InitializeComponent();
		}

		private bool ValidPath
		{
			get => this.validPath;
			set
			{
				this.validPath             = value;
				this.buttonInstall.Enabled = value && this.hasLatest;
			}
		}

		private bool HasLatest
		{
			get => this.hasLatest;
			set
			{
				this.hasLatest             = value;
				this.buttonInstall.Enabled = value && this.validPath;
			}
		}

		private async void MainForm_Load(object sender, EventArgs e)
		{
			await this.LoadLatestVersion();

			this.UpdateRailWorksLocation();
		}

		private void UpdateRailWorksLocation(string location = default)
		{
			this.rwLoc = location ?? this.FindRailWorksLocation();

			if (this.rwLoc != null)
			{
				this.txtRWLoc.Text = this.rwLoc;
				this.ValidPath     = true;

				this.UpdateCurrentVersion();
			}
			else
			{
				this.txtRWLoc.Text = "";
				this.ValidPath     = false;
			}
		}

		private string FindRailWorksLocation()
		{
			var steamKey = Registry.CurrentUser.OpenSubKey("Software\\Valve\\Steam");

			if (steamKey == null)
				return null;

			var steamLoc = Path.GetFullPath((string) steamKey.GetValue("SteamPath", null));

			return Path.Combine(steamLoc, "SteamApps\\Common\\RailWorks\\");
		}

		private void UpdateCurrentVersion()
		{
			var verFile     = Path.Combine(this.rwLoc, Installer.ProjectRoot, "Version.json");
			var versionText = "None";
			this.currentVersion = null;

			if (!Installer.UninstallDirs.Select(d => Path.Combine(this.rwLoc, d)).Any(Directory.Exists))
			{
				this.installState = InstallState.NotInstalled;
			}
			else
			{
				if (File.Exists(verFile))
				{
					var fileContents = File.ReadAllText(verFile);
					var versionInfo  = JsonConvert.DeserializeObject<VersionInfoLocal>(fileContents);

					if (versionInfo != null)
					{
						this.installState   = InstallState.Installed;
						this.currentVersion = versionInfo;
						versionText         = $"Version {versionInfo.Version}, published {versionInfo.PublishDate} (UTC)";
					}
					else
					{
						this.installState = InstallState.LegacyInstalled;
						versionText       = "Unknown (Invalid Version File)";
					}
				}
				else
				{
					this.installState = InstallState.LegacyInstalled;
					versionText       = "Legacy";
				}
			}

			this.Invoke(new Action(() => {
				this.labelCurrent.Text = versionText;
			}));
		}

		private async Task LoadLatestVersion()
		{
			var client = new HttpClient {
				BaseAddress = new Uri(Settings.Default.WebBaseAddress),
				Timeout     = TimeSpan.FromSeconds(2),
			};

			var latest     = await client.GetStringAsync("latest.aspx");
			var latestInfo = JsonConvert.DeserializeObject<VersionInfoRemote>(latest);

			this.latestVersion          = latestInfo;
			this.loadingVersion.Visible = false;
			this.labelLatest.Visible    = true;
			this.HasLatest              = true;

			this.labelLatest.Text = $"Version {latestInfo.Version}, published {latestInfo.PublishDate} (UTC)";
		}

		private void buttonExit_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			Application.Exit();
		}

		private void buttonBrowseRWLoc_Click(object sender, EventArgs e)
		{
			if (this.openRWDialog.ShowDialog() == DialogResult.OK)
			{
				if (Path.GetFileName(this.openRWDialog.FileName).ToLower() == "railworks.exe")
				{
					this.UpdateRailWorksLocation(Path.GetDirectoryName(this.openRWDialog.FileName));
				}
			}
		}

		private void buttonInstall_Click(object sender, EventArgs e)
		{
			var installer     = new Installer(this.latestVersion, this.currentVersion, this.installState, this.rwLoc);
			var installerForm = new InstallerForm(installer);

			installerForm.ShowDialog();

			this.UpdateCurrentVersion();
		}
	}
}