using System.Collections.Generic;
using System.IO;
using Data;
using Optimization;
using Unity.Collections;
using Unity.Jobs;

namespace MapLogic
{
    public static class MapWriter
    {
        public static void SaveMap(string fileName, MapProvider mapProvider)
        {
            Directory.CreateDirectory(Constants.mapFolderPath);
            var filePath = Path.Combine(Constants.mapFolderPath, fileName);
            using var file = File.OpenWrite(filePath);
            WriteMap(mapProvider, file);
        }


        public static void WriteMap(MapProvider mapProvider, Stream stream)
        {
            var result = new List<NativeList<byte>>(mapProvider.MapData.Chunks.Length);
            var blocks = new List<NativeArray<BlockData>>(mapProvider.MapData.Chunks.Length);
            for (var i = 0; i < mapProvider.MapData.Chunks.Length; i++)
            {
                result.Add(new NativeList<byte>(Allocator.TempJob));
                blocks.Add(new NativeArray<BlockData>(mapProvider.MapData.Chunks[i].Blocks, Allocator.TempJob));
            }

            var jobHandles = new NativeList<JobHandle>(Allocator.Temp);
            for (var i = 0; i < mapProvider.MapData.Chunks.Length; i++)
            {
                var job = new ChunkSerializer(result[i],
                    blocks[i],
                    mapProvider.MapData.SolidColor);
                var jobHandle = job.Schedule();
                jobHandles.Add(jobHandle);
            }

            JobHandle.CompleteAll(jobHandles);
            var memoryStream = new MemoryStream();
            var binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write(mapProvider.MapData.Width);
            binaryWriter.Write(mapProvider.MapData.Height);
            binaryWriter.Write(mapProvider.MapData.Depth);
            for (var i = 0; i < mapProvider.MapData.Chunks.Length; i++)
            {
                for (var j = 0; j < result[i].Length; j++)
                {
                    binaryWriter.Write(result[i][j]);
                }

                result[i].Dispose();
                blocks[i].Dispose();
            }

            jobHandles.Dispose();
            stream.Write(memoryStream.ToArray());
        }
    }
}