using System.IO;

namespace CTAInstaller
{
	static class LegacyUninstaller
	{
		public static void UninstallLegacyFrom(string rwLoc)
		{
			foreach (var uninstallDir in Installer.UninstallDirs)
			{
				var rmDir = Path.Combine(rwLoc, uninstallDir);

				if (Directory.Exists(rmDir))
					Directory.Delete(rmDir, true);
			}
		}
	}
}