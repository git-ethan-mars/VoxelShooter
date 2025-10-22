using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Collections;
namespace VoxelMap
{
	public static class MapDataReader
	{
		public static IEnumerable<string> GetExistedMaps()
		{
			return Directory.GetFiles(Constants.MapFolderPath, $"*{Constants.RchExtension}")
				.Union(Directory.GetFiles(Constants.MapFolderPath, $"*{Constants.VxlExtension}"))
				.Select(Path.GetFileNameWithoutExtension).ToList();
		}
		
		public static MapData ReadFromFile(string mapName)
		{
			string rchFilePath = Path.Combine(Constants.MapFolderPath, $"{mapName}{Constants.RchExtension}");
			string vxlFilePath = Path.Combine(Constants.MapFolderPath, $"{mapName}{Constants.VxlExtension}");
			if (File.Exists(rchFilePath))
			{
				using FileStream file = File.OpenRead(rchFilePath);
				return ReadFromStream(file);
			}

			if (File.Exists(vxlFilePath))
			{
				return Vxl2RchConverter.LoadVxl(vxlFilePath);
			}
			
			var voxels = new NativeArray<VoxelData>(16777216, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			var mapData = new MapData(voxels, 512, 64, 512);
			return mapData; 
		}
		
		public static async UniTask<MapData> ReadFromFileAsync(string mapName, CancellationToken token = default)
		{
			string rchFilePath = Path.Combine(Constants.MapFolderPath, $"{mapName}{Constants.RchExtension}");
			string vxlFilePath = Path.Combine(Constants.MapFolderPath, $"{mapName}{Constants.VxlExtension}");
			if (File.Exists(rchFilePath))
			{
				await using FileStream file = File.OpenRead(rchFilePath);
				return await ReadFromStreamAsync(file, token);
			}

			if (File.Exists(vxlFilePath))
			{
				return await Vxl2RchConverter.LoadVxlAsync(vxlFilePath, token);
			}

			var voxels = new NativeArray<VoxelData>(16777216, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			var mapData = new MapData(voxels, 512, 64, 512);
			return mapData;
		}

		public static MapData ReadFromStream(Stream stream)
		{
			stream.Seek(0, SeekOrigin.Begin);
			using var ms = new MemoryStream();
			stream.CopyTo(ms);
			byte[] bytes = ms.ToArray();
			var width = BitConverter.ToInt32(bytes, 0);
			var height = BitConverter.ToInt32(bytes, 4);
			var depth = BitConverter.ToInt32(bytes, 8);
			var voxels = new NativeArray<VoxelData>(width * height * depth, Allocator.Persistent);
			var position = 12;
			int chunksCount = width * height * depth / Chunk.ChunkSizeCubed;
			for (var i = 0; i < chunksCount; i++)
			{
				var mapRun = (MapRun)bytes[position];
				position += 1;
				while (mapRun != MapRun.ChunkEnd)
				{
					if (mapRun == MapRun.Solid)
					{
						var solidStart = BitConverter.ToInt32(bytes, position);
						position += 4;
						var solidEnd = BitConverter.ToInt32(bytes, position);
						position += 4;
						for (int j = solidStart; j <= solidEnd; j++)
						{
							voxels[i * Chunk.ChunkSizeCubed + j] = VoxelData.DefaultInner;
						}
					}

					if (mapRun == MapRun.Colored)
					{
						var coloredStart = BitConverter.ToInt32(bytes, position);
						position += 4;
						var coloredEnd = BitConverter.ToInt32(bytes, position);
						position += 4;
						for (int j = coloredStart; j <= coloredEnd; j++)
						{
							var color = BitConverter.ToUInt32(bytes, position).ToColor32();
							position += 4;
							voxels[i * Chunk.ChunkSizeCubed + j] = new VoxelData(color);
						}
					}

					mapRun = (MapRun)bytes[position];
					position += 1;
				}
			}

			var mapData = new MapData(voxels, width, height, depth);
			return mapData;
		}

		public static async UniTask<MapData> ReadFromStreamAsync(Stream stream, CancellationToken token = default)
		{
			stream.Seek(0, SeekOrigin.Begin);
			using var ms = new MemoryStream();
			await stream.CopyToAsync(ms, token);
			byte[] bytes = ms.ToArray();
			var width = BitConverter.ToInt32(bytes, 0);
			var height = BitConverter.ToInt32(bytes, 4);
			var depth = BitConverter.ToInt32(bytes, 8);
			var voxels = new NativeArray<VoxelData>(width * height * depth, Allocator.Persistent);
			var position = 12;
			int chunksCount = width * height * depth / Chunk.ChunkSizeCubed;
			for (var i = 0; i < chunksCount; i++)
			{
				var mapRun = (MapRun)bytes[position];
				position += 1;
				while (mapRun != MapRun.ChunkEnd)
				{
					if (mapRun == MapRun.Solid)
					{
						var solidStart = BitConverter.ToInt32(bytes, position);
						position += 4;
						var solidEnd = BitConverter.ToInt32(bytes, position);
						position += 4;
						for (int j = solidStart; j <= solidEnd; j++)
						{
							voxels[i * Chunk.ChunkSizeCubed + j] = VoxelData.DefaultInner;
						}
					}

					if (mapRun == MapRun.Colored)
					{
						var coloredStart = BitConverter.ToInt32(bytes, position);
						position += 4;
						var coloredEnd = BitConverter.ToInt32(bytes, position);
						position += 4;
						for (int j = coloredStart; j <= coloredEnd; j++)
						{
							var color = BitConverter.ToUInt32(bytes, position).ToColor32();
							position += 4;
							voxels[i * Chunk.ChunkSizeCubed + j] = new VoxelData(color);
						}
					}

					mapRun = (MapRun)bytes[position];
					position += 1;
				}
			}

			var mapData = new MapData(voxels, width, height, depth);
			return mapData;
		}
	}
}