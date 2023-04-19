using System;
using System.Collections.Generic;
using System.IO;
using Data;
using UnityEngine;

namespace MapLogic
{
    public static class MapReader
    {
        public static Map ReadFromFile(string fileName)
        {
            if (Path.GetExtension(fileName) != ".rch")
            {
                if (Path.GetExtension(fileName) == ".vxl")
                {
                    return Vxl2RchConverter.LoadVxl(fileName);
                }

                throw new ArgumentException("Неправильный путь до файла");
            }

            var filePath = Application.dataPath + $"/Maps/{fileName}";
            if (!File.Exists(filePath))
            {
                return Map.CreateNewMap();
            }

            using var file = File.OpenRead(filePath);
            return ReadFromStream(file);
        }

        public static Map ReadFromStream(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var binaryReader = new BinaryReader(stream);
            var width = binaryReader.ReadInt32();
            var height = binaryReader.ReadInt32();
            var depth = binaryReader.ReadInt32();
            var chunks = new ChunkData[width / ChunkData.ChunkSize * height / ChunkData.ChunkSize * depth /
                                       ChunkData.ChunkSize];
            for (var i = 0; i < chunks.Length; i++)
            {
                chunks[i] = new ChunkData();
            }
            var spawnPoints = new List<SpawnPoint>();
            var map = new Map(new MapData(chunks, width, height, depth, spawnPoints));
            for (var x = 0; x < width; x++)
            {
                for (var z = 0; z < depth; z++)
                {
                    byte type = 0;
                    while (type != 2)
                    {
                        type = binaryReader.ReadByte();
                        if (type == 0)
                        {
                            var coloredStart = binaryReader.ReadByte();
                            var coloredEnd = binaryReader.ReadByte();
                            for (var i = coloredStart; i <= coloredEnd; i++)
                            {
                                var color = BlockColor.UInt32ToColor(binaryReader.ReadUInt32());
                                map.MapData.Chunks[map.FindChunkNumberByPosition(x, i, z)].Blocks[
                                        x % ChunkData.ChunkSize * ChunkData.ChunkSizeSquared +
                                        i % ChunkData.ChunkSize * ChunkData.ChunkSize + z % ChunkData.ChunkSize] =
                                    new BlockData(color);
                            }
                        }

                        if (type == 1)
                        {
                            var solidStart = binaryReader.ReadByte();
                            var solidEnd = binaryReader.ReadByte();
                            for (var i = solidStart; i <= solidEnd; i++)
                            {
                                map.MapData.Chunks[map.FindChunkNumberByPosition(x, i, z)].Blocks[
                                        x % ChunkData.ChunkSize * ChunkData.ChunkSizeSquared +
                                        i % ChunkData.ChunkSize * ChunkData.ChunkSize + z % ChunkData.ChunkSize] =
                                    new BlockData(map.MapData.SolidColor);
                            }
                        }
                    }
                }
            }

            var spawnPointCount = binaryReader.ReadInt32();
            for (var i = 0; i < spawnPointCount; i++)
            {
                var x = binaryReader.ReadInt32();
                var y = binaryReader.ReadInt32();
                var z = binaryReader.ReadInt32();
                spawnPoints.Add(new SpawnPoint() {X = x, Y = y, Z = z});
            }

            return map;
        }
    }
}