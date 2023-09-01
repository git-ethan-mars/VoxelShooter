using System.Collections.Generic;
using System.IO;
using Data;
using UnityEngine;

namespace MapLogic
{
    public static class MapReader
    {
        private const string RchExtension = ".rch";
        private const string VxlExtension = ".vxl";

        public static MapProvider ReadFromFile(string fileName)
        {
            var rchFilePath = Application.dataPath + $"/Maps/{fileName}" + RchExtension;
            var vxlFilePath = Application.dataPath + $"/Maps/{fileName}" + VxlExtension;
            if (File.Exists(rchFilePath))
            {
                using var file = File.OpenRead(rchFilePath);
                return ReadFromStream(file);
            }

            if (File.Exists(vxlFilePath))
            {
                return Vxl2RchConverter.LoadVxl(vxlFilePath);
            }

            return MapProvider.CreateNewMap();
        }


        public static MapProvider ReadFromStream(Stream stream)
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

            var spawnPoints = new List<SpawnPointData>();
            var mapProvider = new MapProvider(new MapData(chunks, width, height, depth, spawnPoints));
            for (var i = 0; i < chunks.Length; i++)
            {
                var mapRun = (MapRun) binaryReader.ReadByte();
                while (mapRun != MapRun.End)
                {
                    if (mapRun == MapRun.Solid)
                    {
                        var solidStart = binaryReader.ReadInt32();
                        var solidEnd = binaryReader.ReadInt32();
                        for (int j = solidStart; j <= solidEnd; j++)
                        {
                            chunks[i].Blocks[j] = new BlockData(mapProvider.MapData.SolidColor);
                        }
                    }

                    if (mapRun == MapRun.Colored)
                    {
                        var coloredStart = binaryReader.ReadInt32();
                        var coloredEnd = binaryReader.ReadInt32();
                        for (int j = coloredStart; j <= coloredEnd; j++)
                        {
                            var color = BlockColor.UInt32ToColor(binaryReader.ReadUInt32());
                            chunks[i].Blocks[j] = new BlockData(color);
                        }
                    }

                    mapRun = (MapRun) binaryReader.ReadByte();
                }
            }

            var spawnPointCount = binaryReader.ReadInt32();
            for (var i = 0; i < spawnPointCount; i++)
            {
                var x = binaryReader.ReadInt32();
                var y = binaryReader.ReadInt32();
                var z = binaryReader.ReadInt32();
                spawnPoints.Add(new SpawnPointData() {X = x, Y = y, Z = z});
            }

            AddWater(mapProvider, new Color32(3, 58, 135, 255));

            return mapProvider;
        }


        private static void AddWater(MapProvider mapProvider, Color32 waterColor)
        {
            for (var x = 0; x < mapProvider.MapData.Width; x++)
            {
                for (var z = 0; z < mapProvider.MapData.Depth; z++)
                {
                    var blocks = mapProvider.MapData.Chunks[mapProvider.FindChunkNumberByPosition(x, 0, z)].Blocks;
                    var block = blocks[
                        (x & (ChunkData.ChunkSize - 1)) * ChunkData.ChunkSizeSquared +
                        (z & (ChunkData.ChunkSize - 1))];
                    if (!block.Color
                            .Equals(BlockColor.empty)) continue;
                    blocks[(x & (ChunkData.ChunkSize - 1)) * ChunkData.ChunkSizeSquared +
                           (z & (ChunkData.ChunkSize - 1))] = new BlockData(waterColor);
                }
            }
        }
    }
}