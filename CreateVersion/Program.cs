using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CTALib;
using Newtonsoft.Json;

namespace CreateVersion
{
	class Program
	{
		static void Main( string[] args )
		{
			new Program().Start();
		}

		private void Start()
		{
			this.LoadLatestVersion()
				.ContinueWith( this.OnLoadLatestVersion )
				.Wait();
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

			latestInfo.Version++;
			latestInfo.PublishDate = DateTime.Now;

			string newVersion = JsonConvert.SerializeObject( (VersionInfoLocal) latestInfo );
			File.WriteAllText( "Version.json", newVersion );
		}
	}
}
