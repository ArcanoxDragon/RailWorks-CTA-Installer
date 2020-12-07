using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CreateVersion.Properties;
using CTALib;
using Newtonsoft.Json;

namespace CreateVersion
{
	class Program
	{
		private static async Task Main()
		{
			var latestVersion = await LoadLatestVersion();
			var latestInfo    = JsonConvert.DeserializeObject<VersionInfoRemote>(latestVersion);

			latestInfo.Version++;
			latestInfo.PublishDate = DateTime.Now;

			var newVersion = JsonConvert.SerializeObject((VersionInfoLocal) latestInfo);

			File.WriteAllText("Version.json", newVersion);
		}

		private static async Task<string> LoadLatestVersion()
		{
			using var client = new HttpClient {
				BaseAddress = new Uri(Settings.Default.WebBaseAddress),
				Timeout     = TimeSpan.FromSeconds(2),
			};

			return await client.GetStringAsync("latest.aspx");
		}
	}
}