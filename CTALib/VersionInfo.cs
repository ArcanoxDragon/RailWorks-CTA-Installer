using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTALib
{
	public class VersionInfoRemote
	{
		public int Version { get; set; }
		public DateTime PublishDate { get; set; }
		public string Url { get; set; }

		public static implicit operator VersionInfoRemote( VersionInfoLocal local )
		{
			return new VersionInfoRemote
			{
				Version = local.Version,
				PublishDate = local.PublishDate,
				Url = null
			};
		}
	}

	public class VersionInfoLocal
	{
		public VersionInfoLocal()
		{
			this.DirectoryList = new List<string>();
		}

		public int Version { get; set; }
		public DateTime PublishDate { get; set; }
		public List<string> DirectoryList { get; set; }

		public static implicit operator VersionInfoLocal( VersionInfoRemote remote )
		{
			return new VersionInfoLocal
			{
				Version = remote.Version,
				PublishDate = remote.PublishDate
			};
		}
	}
}
