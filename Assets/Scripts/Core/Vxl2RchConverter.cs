using System;
using System.Collections.Generic;
using System.IO;
using GamePlay;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core
{
    public static class Vxl2RchConverter
    {
        private const int Width = 512;
        private static int _height;
        private const int Depth = 512;

        public static Map LoadVxl(string mapName)
        {
            var data = File.ReadAllBytes(Application.dataPath + $"/Maps/{mapName}");
            _height = GetMapHeight(data);
            var heightOffset = 0;
            if (_height % ChunkData.ChunkSize != 0)
            {
                heightOffset = -_height;
                _height = _height / ChunkData.ChunkSize * ChunkData.ChunkSize + ChunkData.ChunkSize;
                heightOffset += _height;
            }

            var colors = new Color32[Width * _height * Depth];
            var currentPosition = 0;

            for (var y = 0; y < Depth; ++y)
            {
                for (var x = 0; x < Width; ++x)
                {
                    while (true)
                    {
                        int number4ByteChunks = data[currentPosition];
                        int topColorStart = data[currentPosition + 1];
                        int topColorEnd = data[currentPosition + 2];
                        var z = topColorStart;
                        var colorPosition = currentPosition + 4;
                        uint color;
                        for (; z <= topColorEnd; z++)
                        {
                            color = BitConverter.ToUInt32(data, colorPosition);
                            colorPosition += 4;
                            colors[GetPosition(x, z, y)] = BlockColor.UIntToColor(color);
                        }

                        var bottomLength = topColorEnd - topColorStart + 1;

                        if (number4ByteChunks == 0)
                        {
                            currentPosition += 4 * (bottomLength + 1);
                            break;
                        }

                        var topLength = (number4ByteChunks - 1) - bottomLength;

                        currentPosition += data[currentPosition] * 4;

                        int bottomColorEnd = data[currentPosition + 3];
                        var bottomColorStart = bottomColorEnd - topLength;

                        for (z = bottomColorStart; z < bottomColorEnd; z++)
                        {
                            color = BitConverter.ToUInt32(data, colorPosition);
                            colorPosition += 4;
                            colors[GetPosition(x, z, y)] = BlockColor.UIntToColor(color);
                        }
                    }
                }
            }

            AddInnerVoxels(colors, heightOffset);

            var chunks =
                new ChunkData[Width / ChunkData.ChunkSize * _height / ChunkData.ChunkSize *
                              Depth /
                              ChunkData.ChunkSize];
            for (var i = 0; i < chunks.Length; i++)
            {
                chunks[i] = new ChunkData();
            }

            var map = new Map(chunks, Width, _height, Depth);
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < _height - heightOffset; y++)
                {
                    for (var z = 0; z < Depth; z++)
                    {
                        chunks[map.FindChunkNumberByPosition(
                                new Vector3Int(Width - 1 - x, _height - heightOffset - 1 - y, z))].Blocks[
                                (Width - 1 - x) & (ChunkData.ChunkSize - 1),
                                (_height - heightOffset - 1 - y) & (ChunkData.ChunkSize - 1),
                                z & (ChunkData.ChunkSize - 1)].Color =
                            colors[GetPosition(x, y, z)];
                    }
                }
            }

            map.AddWater();
            return map;
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
                    var lengthBottom = topColorEnd - topColorStart + 1;
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

        private static void AddInnerVoxels(Color32[] colors, int heightOffset)
        {
            for (var y = 0; y < Depth; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var i = 0;
                    while (colors[GetPosition(x, _height - heightOffset - 1 - i, y)]
                           .Equals(BlockColor.Empty))
                    {
                        i++;
                        if (i == _height - heightOffset) break;
                    }

                    if (i == _height - heightOffset) continue;
                    for (var j = 0; j < i; j++)
                        colors[GetPosition(x, _height - heightOffset - 1 - j, y)] =
                            new Color32((byte) (89 + Random.Range(-10, 10)), 53, 47, 255);
                }
            }
        }
    }
}