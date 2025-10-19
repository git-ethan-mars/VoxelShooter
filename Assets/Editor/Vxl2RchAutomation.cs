using System.IO;
using UnityEditor;
using VoxelMap;

namespace Editor
{
	public static class Vxl2RchAutomation
	{
		[MenuItem("Tools/Vxl2RchConverter/Convert all")]
		public static async void ConvertAll()
		{
			string[] vxlFilePaths = Directory.GetFiles(Constants.MapFolderPath, $"*{Constants.VxlExtension}");
			foreach (string vxlPath in vxlFilePaths)
			{
				string fileName = Path.GetFileNameWithoutExtension(Path.GetFileName(vxlPath));
				using MapData mapData = await Vxl2RchConverter.LoadVxlAsync(vxlPath);
				string filePath = Path.Combine(Constants.MapFolderPath, $"{fileName}{Constants.RchExtension}");
				await MapDataWriter.SaveAsync(filePath, mapData);
			}
		}
	}
}