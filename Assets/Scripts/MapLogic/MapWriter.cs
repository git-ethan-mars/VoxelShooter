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
        public static void SaveMap(string fileName, IMapProvider mapProvider)
        {
            Directory.CreateDirectory(Constants.mapFolderPath);
            var filePath = Path.Combine(Constants.mapFolderPath, fileName);
            using var file = File.OpenWrite(filePath);
            WriteMap(mapProvider, file);
        }

        public static void WriteMap(IMapProvider mapProvider, Stream stream)
        {
            var result = new List<NativeList<byte>>(mapProvider.ChunkCount);
            var blocks = new List<NativeArray<BlockData>>(mapProvider.ChunkCount);
            for (var i = 0; i < mapProvider.ChunkCount; i++)
            {
                result.Add(new NativeList<byte>(Allocator.TempJob));
                blocks.Add(new NativeArray<BlockData>(mapProvider.GetChunkByIndex(i).Blocks, Allocator.TempJob));
            }

            var jobHandles = new NativeList<JobHandle>(Allocator.Temp);
            for (var i = 0; i < mapProvider.ChunkCount; i++)
            {
                var job = new ChunkSerializer(result[i],
                    blocks[i],
                    mapProvider.SolidColor);
                var jobHandle = job.Schedule();
                jobHandles.Add(jobHandle);
            }

            JobHandle.CompleteAll(jobHandles);
            using var memoryStream = new MemoryStream();
            using var binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write(mapProvider.Width);
            binaryWriter.Write(mapProvider.Height);
            binaryWriter.Write(mapProvider.Depth);
            for (var i = 0; i < mapProvider.ChunkCount; i++)
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