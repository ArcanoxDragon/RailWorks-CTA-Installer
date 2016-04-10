using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CTALib;
using Newtonsoft.Json;
using SharpCompress.Archive;
using SharpCompress.Reader;

namespace CTAInstaller
{
	class Installer
	{
		public static string[] UninstallDirs = new[] {
			"Assets\\briman0094\\CTA",
			"Assets\\briman0094\\APM",
			"Content\\Routes\\3c0f9815-390a-4f00-8432-60e0cee3f802",
			"Content\\Routes\\741cd0d0-cae8-497c-8574-82df7f86b11b"
		};

		private static string[] rootFolders = new[] {
			"Assets\\briman0094",
			"Content\\Routes"
		};

		public static string ProjectRoot = "Assets\\briman0094\\CTA";

		public delegate void ProgressChangedHandler( string state, double progress );

		public event ProgressChangedHandler ProgressChanged;
		public event Action InstallCompleted, InstallCancelled;

		private string packageFile, rwLocation;
		private bool cancelled;
		private Exception error;
		private VersionInfoRemote versionToInstall;
		private VersionInfoLocal currentInstalled;
		private InstallState currentInstallState;
		private CancellationTokenSource ctSource;

		public Installer( VersionInfoRemote versionToInstall, VersionInfoLocal currentInstalled, InstallState currentInstallState, string rwLocation )
		{
			this.versionToInstall = versionToInstall;
			this.currentInstalled = currentInstalled;
			this.currentInstallState = currentInstallState;
			this.rwLocation = rwLocation;
			this.ctSource = new CancellationTokenSource();

			string tempPath = Path.GetTempPath();
			string dlPath = Path.Combine( tempPath, "CTAInstaller" );
			if ( !Directory.Exists( dlPath ) )
				Directory.CreateDirectory( dlPath );
			this.packageFile = Path.Combine( dlPath, "cta_latest.rwp" );
		}

		private void UpdateProgress( string state, double progress )
		{
			if ( this.ProgressChanged != null )
				this.ProgressChanged( state, progress );
		}

		public async Task Install()
		{
			try
			{
				if ( this.currentInstallState == InstallState.Installed )
					await Uninstall( this.currentInstalled );
				else if ( this.currentInstallState == InstallState.LegacyInstalled )
				{
					this.UpdateProgress( "Uninstalling legacy...please wait", -1 );

					await Task.Run( () => LegacyUninstaller.UninstallLegacyFrom( this.rwLocation ) );
				}

				await DownloadPackage();
				await this.InstallFromPackage();

				if ( !this.cancelled )
					if ( this.InstallCompleted != null )
						this.InstallCompleted();
			}
			catch ( Exception ex )
			{
				if ( this.error == null )
					throw;
				else
					throw new Exception( ex.Message + "\n\n" + this.error.Message, this.error );
			}
		}

		private async Task Uninstall( VersionInfoLocal currentVersion )
		{
			this.UpdateProgress( "Uninstalling old version...", -1 );

			int curEntry = 0, entryCount = currentVersion.DirectoryList.Count;
			foreach ( string directory in currentVersion.DirectoryList )
			{
				string stateString = string.Format( "Uninstalling old version... {0}%", ( ++curEntry / (double) entryCount ).ToString( "0.00" ) );
				this.UpdateProgress( stateString, curEntry / (double) entryCount );

				await Task.Run( () => Directory.Delete( directory, true ) );
			}
		}

		public void Cancel()
		{
			this.cancelled = true;
			this.ctSource.Cancel();

			if ( this.InstallCancelled != null )
				this.InstallCancelled();
		}

		private async Task InstallFromPackage()
		{
			if ( this.ctSource.IsCancellationRequested )
				return;

			if ( this.packageFile == null || !File.Exists( this.packageFile ) )
				throw new Exception( "The installation package could not be found. " );

			List<string> installedFolders = new List<string>();
			string rootFolderPattern = string.Join( "|", rootFolders.Select( Regex.Escape ) );
			Regex folderRegex = new Regex( string.Format( @"^({0})\\([^\\/]+).*$", rootFolderPattern ), RegexOptions.IgnoreCase );

			using ( Stream zipStream = File.Open( this.packageFile, FileMode.Open, FileAccess.Read, FileShare.Read ) )
			{
				IArchive zip = ArchiveFactory.Open( zipStream );

				this.UpdateProgress( "Extracting archive...", -1 );

				int curEntry = 0, entryCount = zip.Entries.Count();

				foreach ( IArchiveEntry entry in zip.Entries )
				{
					if ( this.ctSource.IsCancellationRequested )
						return;

					Match rootMatch = folderRegex.Match( entry.Key );

					if ( rootMatch.Success )
					{
						string root = rootMatch.Groups[ 1 ].Value;
						string key = rootMatch.Groups[ 2 ].Value;
						string folder = Path.Combine( root, key ).ToLower();

						if ( !installedFolders.Contains( folder ) )
							installedFolders.Add( folder );
					}

					string stateString = string.Format( "Extracting archive... {0} / {1} entries", ++curEntry, entryCount );
					this.UpdateProgress( stateString, ( curEntry / (double) entryCount ) );

					string entryDir = Path.GetDirectoryName( entry.Key );
					string extractTo = Path.Combine( this.rwLocation, entryDir );

					if ( !Directory.Exists( extractTo ) )
						Directory.CreateDirectory( extractTo );

					await Task.Run( () => entry.WriteToDirectory( extractTo ), this.ctSource.Token );
				}
			}

			File.Delete( this.packageFile );

			VersionInfoLocal newInstalledVersion = (VersionInfoLocal) this.versionToInstall;
			newInstalledVersion.DirectoryList = installedFolders;

			string verFile = Path.Combine( this.rwLocation, ProjectRoot, "Version.json" );
			string verDir = Path.GetDirectoryName( verFile );

			if ( !Directory.Exists( verDir ) )
				Directory.CreateDirectory( verDir );

			File.WriteAllText( verFile, JsonConvert.SerializeObject( newInstalledVersion ) );
		}

		private async Task DownloadPackage()
		{
			WebClient client = new WebClient();
			client.DownloadProgressChanged += OnDownloadProgressChanged;
			client.DownloadFileCompleted += OnDownloadCompleted;

			await Task.Run( () =>
			{
				Uri root = new Uri( Properties.Settings.Default.WebBaseAddress );
				client.DownloadFileAsync( new Uri( root, this.versionToInstall.Url ), this.packageFile );
				while ( client.IsBusy && !this.ctSource.IsCancellationRequested ) Thread.Sleep( 100 );
				if ( this.ctSource.IsCancellationRequested )
					client.CancelAsync();
			} );
		}

		void OnDownloadCompleted( object sender, System.ComponentModel.AsyncCompletedEventArgs e )
		{
			if ( e.Cancelled || e.Error != null )
			{
				if ( File.Exists( this.packageFile ) )
					File.Delete( this.packageFile );

				this.error = e.Error;
			}
		}

		private void OnDownloadProgressChanged( object sender, DownloadProgressChangedEventArgs e )
		{
			FileSize totalSize = new FileSize( e.TotalBytesToReceive );
			FileSize curSize = new FileSize( e.BytesReceived );
			string stateString = string.Format( "Fetching installation package... {0} / {1}", curSize.ToString(), totalSize.ToString() );

			this.UpdateProgress( stateString, e.ProgressPercentage / 100.0 );
		}

	}
}
