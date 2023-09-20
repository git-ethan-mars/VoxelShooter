﻿using System.Collections.Generic;
using System.IO;
using Data;
using Optimization;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace MapLogic
{
    public static class MapWriter
    {
        private const string RchExtension = ".rch";

        public static void SaveMap(string fileName, MapProvider mapProvider)
        {
            var mapDirectory = Application.dataPath + "/Maps";
            if (!Directory.Exists(mapDirectory))
            {
                Directory.CreateDirectory(mapDirectory);
            }

            var filePath = Application.dataPath + $"/Maps/{fileName + RchExtension}";

            using var file = File.OpenWrite(filePath);
            WriteMap(mapProvider, file);
        }


        public static void WriteMap(MapProvider mapProvider, Stream stream)
        {
            var result = new List<NativeList<byte>>();
            for (var i = 0; i < mapProvider.MapData.Chunks.Length; i++)
            {
                result.Add(new NativeList<byte>(Allocator.TempJob));
            }

            var jobHandles = new NativeList<JobHandle>(Allocator.Temp);
            for (var i = 0; i < mapProvider.MapData.Chunks.Length; i++)
            {
                var job = new ChunkSerializer(result[i],
                    new NativeArray<BlockData>(mapProvider.MapData.Chunks[i].Blocks, Allocator.TempJob),
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
            foreach (var serializedChunk in result)
            {
                for (var i = 0; i < serializedChunk.Length; i++)
                {
                    binaryWriter.Write(serializedChunk[i]);
                }

                serializedChunk.Dispose();
            }

            jobHandles.Dispose();
            stream.Write(memoryStream.ToArray());
        }
    }
}