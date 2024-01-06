using System.IO;
using Data;
using Infrastructure.Services.StaticData;

namespace MapLogic
{
    public static class MapReader
    {
        public static MapProvider ReadFromFile(string mapName, IStaticDataService staticData)
        {
            var rchFilePath = Path.Combine(Constants.mapFolderPath, $"{mapName}{Constants.RchExtension}");
            var vxlFilePath = Path.Combine(Constants.mapFolderPath, $"{mapName}{Constants.VxlExtension}");
            MapProvider mapProvider;
            var mapConfigure = staticData.GetMapConfigure(mapName);

            if (File.Exists(rchFilePath))
            {
                using var file = File.OpenRead(rchFilePath);
                var mapData = ReadFromStream(file, mapConfigure);
                mapProvider = new MapProvider(mapData, mapConfigure);
            }
            else
            {
                mapProvider = File.Exists(vxlFilePath)
                    ? Vxl2RchConverter.LoadVxl(vxlFilePath, mapConfigure)
                    : CreateNewMap(mapConfigure);
            }

            SetWaterLayer(mapProvider);
            return mapProvider;
        }

        public static MapData ReadFromStream(Stream stream, MapConfigure mapConfigure)
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

            var mapData = new MapData(chunks, width, height, depth, mapConfigure.innerColor,
                mapConfigure.waterColor);

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
                            chunks[i].Blocks[j] = new BlockData(mapData.SolidColor);
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

            return mapData;
        }

        private static void SetWaterLayer(MapProvider mapProvider)
        {
            for (var x = 0; x < mapProvider.Width; x++)
            {
                for (var z = 0; z < mapProvider.Depth; z++)
                {
                    mapProvider.SetBlockByGlobalPosition(x, 0, z, new BlockData(mapProvider.WaterColor));
                }
            }
        }

        private static MapProvider CreateNewMap(MapConfigure mapConfigure, int width = 512, int height = 64,
            int depth = 512)
        {
            var chunks = new ChunkData[width / ChunkData.ChunkSize * height / ChunkData.ChunkSize * depth /
                                       ChunkData.ChunkSize];
            for (var i = 0; i < chunks.Length; i++)
            {
                chunks[i] = new ChunkData();
            }

            var mapData = new MapData(chunks, width, height, depth, mapConfigure.innerColor,
                mapConfigure.waterColor);
            var mapProvider = new MapProvider(mapData, mapConfigure);
            return mapProvider;
        }
    }
}