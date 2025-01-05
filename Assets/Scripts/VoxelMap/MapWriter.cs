using System;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.Jobs;

namespace VoxelMap
{
	public static class MapWriter
	{
		public static void SaveMap(string filePath, MapData mapData)
		{
			var folderPath = Path.GetDirectoryName(filePath);
			if (folderPath is null)
			{
				throw new ArgumentException("Invalid map path", nameof(filePath));
			}
			
			Directory.CreateDirectory(folderPath);
			using var file = File.OpenWrite(filePath);
			WriteMap(mapData, file);
		}

		internal static void WriteMap(MapData mapData, Stream stream)
		{
			var result = new List<NativeList<byte>>(mapData.ChunkCount);
			var voxels = new List<NativeArray<VoxelData>>(mapData.ChunkCount);
			for (var i = 0; i < mapData.ChunkCount; i++)
			{
				result.Add(new NativeList<byte>(Allocator.TempJob));
				voxels.Add(mapData.GetChunkDataByIndex(i).Voxels);
			}

			var jobHandles = new NativeArray<JobHandle>(mapData.ChunkCount, Allocator.Temp);
			for (var i = 0; i < mapData.ChunkCount; i++)
			{
				var job = new ChunkSerializer(result[i],
					voxels[i],
					VoxelData.DefaultInner.Color);

				jobHandles[i] = job.Schedule();
			}

			JobHandle.CompleteAll(jobHandles);
			using var binaryWriter = new BinaryWriter(stream);
			binaryWriter.Write(mapData.Width);
			binaryWriter.Write(mapData.Height);
			binaryWriter.Write(mapData.Depth);
			for (var i = 0; i < mapData.ChunkCount; i++)
			{
				for (var j = 0; j < result[i].Length; j++)
				{
					binaryWriter.Write(result[i][j]);
				}

				result[i].Dispose();
				voxels[i].Dispose();
			}

			jobHandles.Dispose();
		}
	}
}