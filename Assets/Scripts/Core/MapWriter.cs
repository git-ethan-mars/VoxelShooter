﻿using System;
using System.IO;
using UnityEngine;

namespace Core
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
            binaryWriter.Write(map.Width);
            binaryWriter.Write(map.Height);
            binaryWriter.Write(map.Depth);
            for (var x = 0; x < map.Width / ChunkData.ChunkSize; x++)
            {
                for (var y = 0; y < map.Height / ChunkData.ChunkSize; y++)
                {
                    for (var z = 0; z < map.Depth / ChunkData.ChunkSize; z++)
                    {
                        WriteChunk(z + y * (map.Depth / ChunkData.ChunkSize) +
                                   x * (map.Height / ChunkData.ChunkSize * map.Depth / ChunkData.ChunkSize));
                    }
                }
            }

            void WriteChunk(int chunkNumber)
            {
                for (var x = 0; x < ChunkData.ChunkSize; x++)
                {
                    for (var y = 0; y < ChunkData.ChunkSize; y++)
                    {
                        for (var z = 0; z < ChunkData.ChunkSize; z++)
                        {
                            var color = map.Chunks[chunkNumber]
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