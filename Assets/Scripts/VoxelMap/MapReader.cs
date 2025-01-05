using System;
using System.IO;

namespace VoxelMap
{
    public static class MapReader
    {
        public static MapData ReadFromFile(string mapName)
        {
            var rchFilePath = Path.Combine(Constants.MapFolderPath, $"{mapName}{Constants.RchExtension}");
            var vxlFilePath = Path.Combine(Constants.MapFolderPath, $"{mapName}{Constants.VxlExtension}");
            if (File.Exists(rchFilePath))
            {
                using var file = File.OpenRead(rchFilePath);
                return ReadFromStream(file);
            }

            if (File.Exists(vxlFilePath))
            {
                return Vxl2RchConverter.LoadVxl(vxlFilePath);
            }

            return CreateNewMap();
        }

        public static MapData ReadFromStream(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            var bytes = ms.ToArray();
            var width = BitConverter.ToInt32(bytes, 0);
            var height = BitConverter.ToInt32(bytes, 4);
            var depth = BitConverter.ToInt32(bytes, 8);
            var chunks = new ChunkData[width / ChunkData.ChunkSize * height / ChunkData.ChunkSize * depth /
                                       ChunkData.ChunkSize];

            for (var i = 0; i < chunks.Length; i++)
            {
                chunks[i] = new ChunkData();
            }

            var position = 12;
            for (var i = 0; i < chunks.Length; i++)
            {
                var mapRun = (MapRun)bytes[position];
                position += 1;
                while (mapRun != MapRun.End)
                {
                    if (mapRun == MapRun.Solid)
                    {
                        var solidStart = BitConverter.ToInt32(bytes, position);
                        position += 4;
                        var solidEnd = BitConverter.ToInt32(bytes, position);
                        position += 4;
                        for (int j = solidStart; j <= solidEnd; j++)
                        {
                            chunks[i].Voxels[j] = VoxelData.DefaultInner;
                        }
                    }

                    if (mapRun == MapRun.Colored)
                    {
                        var coloredStart = BitConverter.ToInt32(bytes, position);
                        position += 4;
                        var coloredEnd = BitConverter.ToInt32(bytes, position);
                        position += 4;
                        for (int j = coloredStart; j <= coloredEnd; j++)
                        {
                            var color = BitConverter.ToUInt32(bytes, position).ToColor32();
                            position += 4;
                            chunks[i].Voxels[j] = new VoxelData(color);
                        }
                    }

                    mapRun = (MapRun)bytes[position];
                    position += 1;
                }
            }

            var mapData = new MapData(chunks, width, height, depth);
            return mapData;
        }

        private static MapData CreateNewMap(int width = 512, int height = 64,
            int depth = 512)
        {
            var chunks = new ChunkData[width / ChunkData.ChunkSize * height / ChunkData.ChunkSize * depth /
                                       ChunkData.ChunkSize];

            for (var i = 0; i < chunks.Length; i++)
            {
                chunks[i] = new ChunkData();
            }
            var mapData = new MapData(chunks, width, height, depth);
            return mapData;
        }
    }
}