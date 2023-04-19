using System;
using System.IO;
using Data;
using Optimization;
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
            for (var x = 0; x < map.MapData.Width; x++)
            {
                for (var z = 0; z < map.MapData.Depth; z++)
                {
                    var coloredStart = -1;
                    var coloredEnd = -1;
                    var solidStart = -1;
                    var solidEnd = -1;
                    for (var y = 0; y < map.MapData.Height; y++)
                    {
                        var block = map.GetBlockByGlobalPosition(x, y, z);
                        if (block.Color.IsEquals(map.MapData.SolidColor))
                        {
                            if (solidStart == -1)
                            {
                                solidStart = y;
                                solidEnd = y;
                            }
                            else
                            {
                                solidEnd++;
                            }

                            TryWriteColoredRun(map, binaryWriter, x, z, ref coloredStart, ref coloredEnd);
                            continue;
                        }

                        if (block.Color.IsEquals(BlockColor.Empty))
                        {
                            TryWriteColoredRun(map, binaryWriter, x, z, ref coloredStart, ref coloredEnd);
                            TryWriteSolidRun(binaryWriter, ref solidStart, ref solidEnd);
                        }
                        else
                        {
                            if (coloredStart == -1)
                            {
                                coloredStart = y;
                                coloredEnd = y;
                            }
                            else
                            {
                                coloredEnd++;
                            }
                            TryWriteSolidRun(binaryWriter, ref solidStart, ref solidEnd);
                        }
                    }
                    TryWriteColoredRun(map, binaryWriter, x, z, ref coloredStart, ref coloredEnd);
                    TryWriteSolidRun(binaryWriter, ref solidStart, ref solidEnd);
                    binaryWriter.Write((byte)2);
                }
                
            }
            binaryWriter.Write(map.MapData.SpawnPoints.Count);
            for (var i = 0; i < map.MapData.SpawnPoints.Count; i++)
            {
                var spawnPoint = map.MapData.SpawnPoints[i];
                binaryWriter.Write(spawnPoint.X);
                binaryWriter.Write(spawnPoint.Y);
                binaryWriter.Write(spawnPoint.Z);
            }
        }

        private static void TryWriteSolidRun(BinaryWriter binaryWriter, ref int solidStart, ref int solidEnd)
        {
            if (solidStart == -1) return;
            binaryWriter.Write((byte) 1);
            binaryWriter.Write((byte) solidStart);
            binaryWriter.Write((byte) solidEnd);
            solidStart = -1;
            solidEnd = -1;
        }

        private static void TryWriteColoredRun(Map map, BinaryWriter binaryWriter, int x, int z, ref int coloredStart,
            ref int coloredEnd)
        {
            if (coloredStart == -1) return;
            binaryWriter.Write((byte) 0);
            binaryWriter.Write((byte) coloredStart);
            binaryWriter.Write((byte) coloredEnd);
            for (var i = coloredStart; i <= coloredEnd; i++)
            {
                var block = map.GetBlockByGlobalPosition(x, i, z);
                binaryWriter.Write(block.Color.b);
                binaryWriter.Write(block.Color.g);
                binaryWriter.Write(block.Color.r);
                binaryWriter.Write(block.Color.a);
            }

            coloredStart = -1;
            coloredEnd = -1;
        }
    }
}