using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.IO;
using CTALib;

namespace CTAInstaller
{
	public partial class MainForm : Form
	{
		private string rwLoc;
		private InstallState installState = InstallState.NotInstalled;
		private VersionInfoLocal currentVersion;
		private VersionInfoRemote latestVersion;
		private bool hasLatest, validPath;

		public MainForm()
		{
			InitializeComponent();
		}

		private bool ValidPath
		{
			get
			{
				return this.validPath;
			}
			set
			{
				this.validPath = value;
				this.buttonInstall.Enabled = value && this.hasLatest;
			}
		}

		private bool HasLatest
		{
			get
			{
				return this.hasLatest;
			}
			set
			{
				this.hasLatest = value;
				this.buttonInstall.Enabled = value && this.validPath;
			}
		}

		private void MainForm_Load( object sender, EventArgs e )
		{
			Task.Run( (Func<Task<string>>) this.LoadLatestVersion )
				.ContinueWith( this.OnLoadLatestVersion );

			Task.Run( (Func<string>) this.DetectRWLocation )
				.ContinueWith( async task => this.OnSetRWLocation( await task ) );
		}

		private string DetectRWLocation()
		{
			RegistryKey steamKey = Registry.CurrentUser.OpenSubKey( "Software\\Valve\\Steam" );
			string steamLoc = Path.GetFullPath( (string) steamKey.GetValue( "SteamPath", null ) );

			if ( steamLoc != null )
			{
				return Path.Combine( steamLoc, "SteamApps\\Common\\RailWorks\\" );
			}

			return null;
		}

		private void OnSetRWLocation( string rwLocation )
		{
			this.rwLoc = rwLocation;

			if ( rwLocation != null )
			{
				this.Invoke( new Action( () =>
				{
					this.txtRWLoc.Text = rwLocation;
					this.ValidPath = true;
				} ) );

				Task.Run( (Action) this.DetectCurrentVersion );
			}
			else
			{
				this.Invoke( new Action( () =>
				{
					this.txtRWLoc.Text = "";
					this.ValidPath = false;
				} ) );
			}
		}

		private void DetectCurrentVersion()
		{
			string verFile = Path.Combine( this.rwLoc, Installer.ProjectRoot, "Version.json" );
			string versionText = "None";
			this.currentVersion = null;

			if ( !Installer.UninstallDirs.Select( d => Path.Combine( this.rwLoc, d ) ).Any( Directory.Exists ) )
			{
				this.installState = InstallState.NotInstalled;
			}
			else
			{
				if ( File.Exists( verFile ) )
				{
					string fileContents = File.ReadAllText( verFile );
					VersionInfoLocal versionInfo = JsonConvert.DeserializeObject<VersionInfoLocal>( fileContents );

					if ( versionInfo != null )
					{
						this.installState = InstallState.Installed;
						this.currentVersion = versionInfo;
						versionText = string.Format( "Version {0}, published {1} (UTC)", versionInfo.Version, versionInfo.PublishDate );
					}
					else
					{
						this.installState = InstallState.LegacyInstalled;
						versionText = "Unknown (Invalid Version File)";
					}
				}
				else
				{
					this.installState = InstallState.LegacyInstalled;
					versionText = "Legacy";
				}
			}

			this.Invoke( new Action( () =>
			{
				this.labelCurrent.Text = versionText;
			} ) );
		}

		private Task<string> LoadLatestVersion()
		{
			HttpClient client = new HttpClient();
			client.BaseAddress = new Uri( Properties.Settings.Default.WebBaseAddress );
			client.Timeout = TimeSpan.FromSeconds( 2 );

			return client.GetStringAsync( "latest.aspx" );
		}

		private async void OnLoadLatestVersion( Task<string> loadTask )
		{
			string latest = await loadTask;
			VersionInfoRemote latestInfo = JsonConvert.DeserializeObject<VersionInfoRemote>( latest );
			this.latestVersion = latestInfo;

			this.Invoke( new Action( () =>
			{
				this.loadingVersion.Visible = false;
				this.labelLatest.Visible = true;
				this.HasLatest = true;

				this.labelLatest.Text = string.Format( "Version {0}, published {1} (UTC)", latestInfo.Version, latestInfo.PublishDate );
			} ) );
		}

		private void buttonExit_Click( object sender, EventArgs e )
		{
			Application.Exit();
		}

		private void MainForm_FormClosed( object sender, FormClosedEventArgs e )
		{
			Application.Exit();
		}

		private void buttonBrowseRWLoc_Click( object sender, EventArgs e )
		{
			if ( this.openRWDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK )
			{
				if ( Path.GetFileName( this.openRWDialog.FileName ).ToLower() == "railworks.exe" )
				{
					this.OnSetRWLocation( Path.GetDirectoryName( this.openRWDialog.FileName ) );
				}
			}
		}

		private void buttonInstall_Click( object sender, EventArgs e )
		{
			Installer installer = new Installer( this.latestVersion, this.currentVersion, this.installState, this.rwLoc );
			InstallerForm installerForm = new InstallerForm( installer );

			installerForm.ShowDialog();

			Task.Run( (Action) this.DetectCurrentVersion );
		}
	}
}
