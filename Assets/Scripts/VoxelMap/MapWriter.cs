using System;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.Jobs;

namespace VoxelMap
{
	internal static class MapWriter
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

		public static void WriteMap(MapData mapData, Stream stream)
		{
			var result = new List<NativeList<byte>>(mapData.ChunkCount);
			var blocks = new List<NativeArray<BlockData>>(mapData.ChunkCount);
			for (var i = 0; i < mapData.ChunkCount; i++)
			{
				result.Add(new NativeList<byte>(Allocator.TempJob));
				blocks.Add(new NativeArray<BlockData>(mapData.GetChunkDataByIndex(i).Blocks, Allocator.TempJob));
			}

			using var jobHandles = new NativeList<JobHandle>(Allocator.Temp);
			for (var i = 0; i < mapData.ChunkCount; i++)
			{
				var job = new ChunkSerializer(result[i],
					blocks[i],
					BlockData.Inner.Color);
				var jobHandle = job.Schedule();
				jobHandles.Add(jobHandle);
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
				blocks[i].Dispose();
			}
		}
	}
}