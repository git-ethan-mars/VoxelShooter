using System;
using System.IO;
using Data;
using UnityEngine;

namespace MapLogic
{
    public static class MapWriter
    {
        public static void SaveMap(string fileName, Map map)
        {
            if (Path.GetExtension(fileName) != ".rch")
            {
                throw new ArgumentException();
            }

            var mapDirectory = Application.dataPath + "/Maps";
            if (!Directory.Exists(mapDirectory))
            {
                Directory.CreateDirectory(mapDirectory);
            }

            var filePath = Application.dataPath + $"/Maps/{fileName}";

            using var file = File.OpenWrite(filePath);
            WriteMap(map, file);
        }

        public static void WriteMap(Map map, Stream stream)
        {
            var binaryWriter = new BinaryWriter(stream);
            binaryWriter.Write(map.MapData.Width);
            binaryWriter.Write(map.MapData.Height);
            binaryWriter.Write(map.MapData.Depth);
            for (var x = 0; x < map.MapData.Width / ChunkData.ChunkSize; x++)
            {
                for (var y = 0; y < map.MapData.Height / ChunkData.ChunkSize; y++)
                {
                    for (var z = 0; z < map.MapData.Depth / ChunkData.ChunkSize; z++)
                    {
                        WriteChunk(z + y * (map.MapData.Depth / ChunkData.ChunkSize) +
                                   x * (map.MapData.Height / ChunkData.ChunkSize * map.MapData.Depth / ChunkData.ChunkSize));
                    }
                }
            }
            
            binaryWriter.Write(map.MapData.SpawnPoints.Count);
            for (var i = 0; i < map.MapData.SpawnPoints.Count; i++)
            {
                binaryWriter.Write(map.MapData.SpawnPoints[i].X);
                binaryWriter.Write(map.MapData.SpawnPoints[i].Y);
                binaryWriter.Write(map.MapData.SpawnPoints[i].Z);
            }

            void WriteChunk(int chunkNumber)
            {
                for (var x = 0; x < ChunkData.ChunkSize; x++)
                {
                    for (var y = 0; y < ChunkData.ChunkSize; y++)
                    {
                        for (var z = 0; z < ChunkData.ChunkSize; z++)
                        {
                            var color = map.MapData.Chunks[chunkNumber]
                                .Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].Color;
                            binaryWriter.Write(color.b);
                            binaryWriter.Write(color.g);
                            binaryWriter.Write(color.r);
                            binaryWriter.Write(color.a);
                        }
                    }
                }
            }
        }
    }
}