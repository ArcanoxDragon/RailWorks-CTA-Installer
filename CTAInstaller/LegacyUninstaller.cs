using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTAInstaller
{
	static class LegacyUninstaller
	{
		public static void UninstallLegacyFrom( string rwLoc )
		{
			foreach ( string uninstallDir in Installer.UninstallDirs )
			{
				string rmDir = Path.Combine( rwLoc, uninstallDir );

				if ( Directory.Exists( rmDir ) )
					Directory.Delete( rmDir, true );
			}
		}

	}
}
