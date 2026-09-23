using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using VoxelMap;

namespace Editor
{
	public class BuildPostprocessor
	{
		private const string SteamAppIdFileName = "steam_appid.txt";
		private const string MapFolder = "Maps";
		private const string BuildDataSuffix = "_Data";

		[PostProcessBuild]
		public static void OnPostprocessBuild(BuildTarget target, string pathToExecutionFile)
		{
			string pathToBuildFolder = Path.GetDirectoryName(pathToExecutionFile);
			File.Copy(Path.Combine(Path.GetDirectoryName(Application.dataPath)!, SteamAppIdFileName),
				Path.Combine(pathToBuildFolder!, SteamAppIdFileName), true);
			string pathToMaps = Path.Combine(Application.dataPath, MapFolder);
			string pathToDataFolder = Path.Combine(pathToBuildFolder!, Application.productName + BuildDataSuffix);
			Directory.CreateDirectory(Path.Combine(pathToDataFolder, MapFolder));
			foreach (string mapFileName in Directory.GetFiles(pathToMaps, $"*{Constants.RchExtension}")
				         .Union(Directory.GetFiles(pathToMaps, $"*{Constants.VxlExtension}")))
			{
				File.Copy(Path.Combine(mapFileName),
					Path.Combine(pathToDataFolder, MapFolder, Path.GetFileName(mapFileName)),
					true);
			}
		}
	}
}
