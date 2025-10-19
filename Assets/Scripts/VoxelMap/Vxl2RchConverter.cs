using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
namespace VoxelMap
{
	public static class Vxl2RchConverter
	{
		private const int Width = 512;
		private const int Depth = 512;
		private static int _height;

		public static async UniTask<MapData> LoadVxlAsync(string mapPath)
		{
			byte[] data = await File.ReadAllBytesAsync(mapPath);
			var voxels = LoadVxlCore(data);
			var mapData = new MapData(voxels, Width, _height, Depth);
			return mapData;
		}

		public static MapData LoadVxl(string mapPath)
		{
			byte[] data = File.ReadAllBytes(mapPath);
			var voxels = LoadVxlCore(data);
			var mapData = new MapData(voxels, Width, _height, Depth);
			return mapData;
		}

		private static NativeArray<VoxelData> LoadVxlCore(byte[] data)
		{

			_height = GetMapHeight(data);
			var heightOffset = 0;
			if (_height % Chunk.ChunkSize != 0)
			{
				heightOffset = -_height;
				_height = _height / Chunk.ChunkSize * Chunk.ChunkSize + Chunk.ChunkSize;
				heightOffset += _height;
			}

			var colors = new Color32[Width * _height * Depth];
			var currentPosition = 0;

			for (var y = 0; y < Depth; ++y)
			{
				for (var x = 0; x < Width; ++x)
				{
					var z = 0;
					for (; z < _height; ++z)
					{
						colors[GetPosition(x, z, y)] = new Color32(89, 53, 47, 255);
					}

					z = 0;
					while (true)
					{
						int number4ByteChunks = data[currentPosition];
						int topColorStart = data[currentPosition + 1];
						int topColorEnd = data[currentPosition + 2];
						int colorPosition = currentPosition + 4;
						for (; z < topColorStart; ++z)
						{
							colors[GetPosition(x, z, y)] = VoxelData.Air.Color;
						}

						for (; z <= topColorEnd; z++)
						{
							var packedColor = BitConverter.ToUInt32(data, colorPosition);
							colorPosition += 4;
							colors[GetPosition(x, z, y)] = packedColor.ToColor32();
						}

						int bottomLength = topColorEnd - topColorStart + 1;

						if (number4ByteChunks == 0)
						{
							currentPosition += 4 * (bottomLength + 1);
							break;
						}

						int topLength = number4ByteChunks - 1 - bottomLength;

						currentPosition += data[currentPosition] * 4;

						int bottomColorEnd = data[currentPosition + 3];
						int bottomColorStart = bottomColorEnd - topLength;

						for (z = bottomColorStart; z < bottomColorEnd; z++)
						{
							var packedColor = BitConverter.ToUInt32(data, colorPosition);
							colorPosition += 4;
							colors[GetPosition(x, z, y)] = packedColor.ToColor32();
						}
					}
				}
			}

			var voxels = new NativeArray<VoxelData>(Width * _height * Depth, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			for (var x = 0; x < Width; x++)
			{
				for (var y = 0; y < _height - heightOffset; y++)
				{
					for (var z = 0; z < Depth; z++)
					{
						int index = (Width - 1 - x) * _height * Depth + (_height - heightOffset - 1 - y) * Depth + z;
						voxels[index] = new VoxelData(colors[GetPosition(x, y, z)]);
					}
				}
			}
			return voxels;
		}


		private static int GetPosition(int x, int y, int z)
		{
			return x * _height * Depth + y * Depth + z;
		}

		private static int GetMapHeight(IReadOnlyList<byte> data)
		{
			var position = 0;
			var height = 0;
			for (var y = 0; y < Depth; y++)
				for (var x = 0; x < Width; x++)
				{
					while (true)
					{
						int number4ByteChunks = data[position];
						int topColorStart = data[position + 1];
						int topColorEnd = data[position + 2];
						height = Math.Max(height, topColorEnd + 1);
						int lengthBottom = topColorEnd - topColorStart + 1;
						if (number4ByteChunks == 0)
						{
							position += 4 * (lengthBottom + 1);
							break;
						}

						position += data[position] * 4;
					}
				}

			return height;
		}
	}
}