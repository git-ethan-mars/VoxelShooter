using System;
using System.IO;
using Cysharp.Threading.Tasks;
namespace VoxelMap
{
	public static class MapDataWriter
	{
		public static async UniTask SaveAsync(string filePath, MapData mapData)
		{
			string folderPath = Path.GetDirectoryName(filePath);
			if (folderPath is null)
			{
				throw new ArgumentException("Invalid map path", nameof(filePath));
			}

			Directory.CreateDirectory(folderPath);
			await using FileStream file = File.OpenWrite(filePath);
			byte[] bytes = await mapData.SerializeAsync();
			await file.WriteAsync(bytes);
		}
	}
}