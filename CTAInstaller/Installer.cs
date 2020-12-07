using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CTAInstaller.Properties;
using CTALib;
using Newtonsoft.Json;
using SharpCompress.Archives;

namespace CTAInstaller
{
	class Installer
	{
		public static string[] UninstallDirs = {
			"Assets\\briman0094\\CTA",
			"Assets\\briman0094\\APM",
			"Content\\Routes\\3c0f9815-390a-4f00-8432-60e0cee3f802",
			"Content\\Routes\\741cd0d0-cae8-497c-8574-82df7f86b11b"
		};

		private static readonly string[] RootFolders = {
			"Assets\\briman0094",
			"Content\\Routes"
		};

		public const string ProjectRoot = "Assets\\briman0094\\CTA";

		public delegate void ProgressChangedHandler(string state, double progress);

		public event ProgressChangedHandler ProgressChanged;
		public event Action                 InstallCompleted, InstallCancelled;

		private readonly string                  packageFile;
		private readonly string                  rwLocation;
		private readonly VersionInfoRemote       versionToInstall;
		private readonly VersionInfoLocal        currentInstalled;
		private readonly InstallState            currentInstallState;
		private readonly CancellationTokenSource ctSource;

		private bool      cancelled;
		private Exception error;

		public Installer(VersionInfoRemote versionToInstall, VersionInfoLocal currentInstalled, InstallState currentInstallState, string rwLocation)
		{
			this.versionToInstall    = versionToInstall;
			this.currentInstalled    = currentInstalled;
			this.currentInstallState = currentInstallState;
			this.rwLocation          = rwLocation;
			this.ctSource            = new CancellationTokenSource();

			var tempPath = Path.GetTempPath();
			var dlPath   = Path.Combine(tempPath, "CTAInstaller");
			if (!Directory.Exists(dlPath))
				Directory.CreateDirectory(dlPath);
			this.packageFile = Path.Combine(dlPath, "cta_latest.rwp");
		}

		private void UpdateProgress(string state, double progress)
		{
			this.ProgressChanged?.Invoke(state, progress);
		}

		public async Task Install()
		{
			try
			{
				if (this.currentInstallState == InstallState.Installed)
				{
					await this.Uninstall(this.currentInstalled);
				}
				else if (this.currentInstallState == InstallState.LegacyInstalled)
				{
					this.UpdateProgress("Uninstalling legacy...please wait", -1);

					await Task.Run(() => LegacyUninstaller.UninstallLegacyFrom(this.rwLocation));
				}

				await this.DownloadPackage();
				await this.InstallFromPackage();

				if (!this.cancelled)
				{
					this.InstallCompleted?.Invoke();
				}
			}
			catch (Exception ex)
			{
				if (this.error == null)
					throw;

				throw new Exception(ex.Message + "\n\n" + this.error.Message, this.error);
			}
		}

		private async Task Uninstall(VersionInfoLocal currentVersion)
		{
			this.UpdateProgress("Uninstalling old version...", -1);

			int curEntry = 0, entryCount = currentVersion.DirectoryList.Count;
			foreach (var directory in currentVersion.DirectoryList)
			{
				var stateString = $"Uninstalling old version... {( ++curEntry / (double) entryCount ):0.00}%";
				this.UpdateProgress(stateString, curEntry / (double) entryCount);

				await Task.Run(() => Directory.Delete(directory, true));
			}
		}

		public void Cancel()
		{
			this.cancelled = true;
			this.ctSource.Cancel();

			this.InstallCancelled?.Invoke();
		}

		private async Task InstallFromPackage()
		{
			if (this.ctSource.IsCancellationRequested)
				return;

			if (this.packageFile == null || !File.Exists(this.packageFile))
				throw new Exception("The installation package could not be found. ");

			var installedFolders  = new List<string>();
			var rootFolderPattern = string.Join("|", RootFolders.Select(Regex.Escape));
			var folderRegex       = new Regex($@"^({rootFolderPattern})\\([^\\/]+).*$", RegexOptions.IgnoreCase);

			using (Stream zipStream = File.Open(this.packageFile, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				var zip = ArchiveFactory.Open(zipStream);

				this.UpdateProgress("Extracting archive...", -1);

				var curEntry   = 0;
				var entryCount = zip.Entries.Count();

				foreach (var entry in zip.Entries)
				{
					if (this.ctSource.IsCancellationRequested)
						return;

					var rootMatch = folderRegex.Match(entry.Key);

					if (rootMatch.Success)
					{
						var root   = rootMatch.Groups[1].Value;
						var key    = rootMatch.Groups[2].Value;
						var folder = Path.Combine(root, key).ToLower();

						if (!installedFolders.Contains(folder))
							installedFolders.Add(folder);
					}

					var stateString = $"Extracting archive... {++curEntry} / {entryCount} entries";
					this.UpdateProgress(stateString, ( curEntry / (double) entryCount ));

					var entryDir  = Path.GetDirectoryName(entry.Key);
					var extractTo = Path.Combine(this.rwLocation, entryDir);

					if (!Directory.Exists(extractTo))
						Directory.CreateDirectory(extractTo);

					await Task.Run(() => entry.WriteToDirectory(extractTo), this.ctSource.Token);
				}
			}

			File.Delete(this.packageFile);

			var newInstalledVersion = (VersionInfoLocal) this.versionToInstall;
			newInstalledVersion.DirectoryList = installedFolders;

			var verFile = Path.Combine(this.rwLocation, ProjectRoot, "Version.json");
			var verDir  = Path.GetDirectoryName(verFile);

			if (!Directory.Exists(verDir))
				Directory.CreateDirectory(verDir);

			File.WriteAllText(verFile, JsonConvert.SerializeObject(newInstalledVersion));
		}

		private async Task DownloadPackage()
		{
			var client = new WebClient();
			client.DownloadProgressChanged += this.OnDownloadProgressChanged;
			client.DownloadFileCompleted   += this.OnDownloadCompleted;

			await Task.Run(() => {
				var root = new Uri(Settings.Default.WebBaseAddress);
				client.DownloadFileAsync(new Uri(root, this.versionToInstall.Url), this.packageFile);
				while (client.IsBusy && !this.ctSource.IsCancellationRequested) Thread.Sleep(100);
				if (this.ctSource.IsCancellationRequested)
					client.CancelAsync();
			});
		}

		private void OnDownloadCompleted(object sender, AsyncCompletedEventArgs e)
		{
			if (e.Cancelled || e.Error != null)
			{
				if (File.Exists(this.packageFile))
					File.Delete(this.packageFile);

				this.error = e.Error;
			}
		}

		private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			var totalSize   = new FileSize(e.TotalBytesToReceive);
			var curSize     = new FileSize(e.BytesReceived);
			var stateString = $"Fetching installation package... {curSize} / {totalSize}";

			this.UpdateProgress(stateString, e.ProgressPercentage / 100.0);
		}
	}
}