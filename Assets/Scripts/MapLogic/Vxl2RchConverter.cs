using System;
using System.Collections.Generic;
using System.IO;
using Data;
using UnityEngine;

namespace MapLogic
{
    public static class Vxl2RchConverter
    {
        private const int Width = 512;
        private static int _height;
        private const int Depth = 512;

        public static MapProvider LoadVxl(string mapName)
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
                        var colorPosition = currentPosition + 4;
                        uint color;
                        for (; z < topColorStart; ++z)
                        {
                            colors[GetPosition(x, z, y)] = BlockColor.empty;
                        }

                        for (; z <= topColorEnd; z++)
                        {
                            color = BitConverter.ToUInt32(data, colorPosition);
                            colorPosition += 4;
                            colors[GetPosition(x, z, y)] = BlockColor.UInt32ToColor(color);
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
                            colors[GetPosition(x, z, y)] = BlockColor.UInt32ToColor(color);
                        }
                    }
                }
            }

            var chunks =
                new ChunkData[Width / ChunkData.ChunkSize * _height / ChunkData.ChunkSize *
                              Depth /
                              ChunkData.ChunkSize];

            for (var i = 0; i < chunks.Length; i++)
            {
                chunks[i] = new ChunkData();
            }

            var mapProvider = new MapProvider(new MapData(chunks, Width, _height, Depth, new List<SpawnPointData>()));
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < _height - heightOffset; y++)
                {
                    for (var z = 0; z < Depth; z++)
                    {
                        var chunk = chunks[mapProvider.FindChunkNumberByPosition(
                            Width - 1 - x, _height - heightOffset - 1 - y, z)];
                        chunk.Blocks[((Width - 1 - x) & (ChunkData.ChunkSize - 1)) * ChunkData.ChunkSizeSquared +
                                     ((_height - heightOffset - 1 - y) & (ChunkData.ChunkSize - 1)) *
                                     ChunkData.ChunkSize +
                                     (z & (ChunkData.ChunkSize - 1))] = new BlockData(colors[GetPosition(x, y, z)]);
                    }
                }
            }

            return mapProvider;
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
    }
}