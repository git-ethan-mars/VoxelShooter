using System.IO;
using Common.Extensions;

namespace VoxelMap
{
    public static class MapReader
    {
        public const string VxlExtension = ".vxl";
        public const string RchExtension = ".rch";
        
        public static MapData ReadFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                var extension = Path.GetExtension(filePath);
                
                switch (extension)
                {
                    case RchExtension:
                    {
                        using var file = File.OpenRead(filePath);
                        return ReadFromStream(file);
                    }
                    case VxlExtension:
                        return Vxl2RchConverter.LoadVxl(filePath);
                }
            }
            
            return CreateNewMap();
        }

        public static MapData ReadFromStream(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using var binaryReader = new BinaryReader(stream);
            var width = binaryReader.ReadInt32();
            var height = binaryReader.ReadInt32();
            var depth = binaryReader.ReadInt32();
            var chunks = new ChunkData[width / ChunkData.ChunkSize * height / ChunkData.ChunkSize * depth /
                                       ChunkData.ChunkSize];
            for (var i = 0; i < chunks.Length; i++)
            {
                chunks[i] = new ChunkData();
            }

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
                            chunks[i].Blocks[j] = BlockData.Inner;
                        }
                    }

                    if (mapRun == MapRun.Colored)
                    {
                        var coloredStart = binaryReader.ReadInt32();
                        var coloredEnd = binaryReader.ReadInt32();
                        for (int j = coloredStart; j <= coloredEnd; j++)
                        {
                            var color = binaryReader.ReadUInt32().ToColor32();
                            chunks[i].Blocks[j] = new BlockData(color);
                        }
                    }

                    mapRun = (MapRun) binaryReader.ReadByte();
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