using System;
using System.Collections.Generic;
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
        public static void SaveMap(string fileName, MapProvider mapProvider)
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
                var job = new ChunkSerializer(result[i], mapProvider.MapData.Chunks[i].Blocks,
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
            binaryWriter.Write(mapProvider.MapData.SpawnPoints.Count);
            for (var i = 0; i < mapProvider.MapData.SpawnPoints.Count; i++)
            {
                binaryWriter.Write(mapProvider.MapData.SpawnPoints[i].X);
                binaryWriter.Write(mapProvider.MapData.SpawnPoints[i].Y);
                binaryWriter.Write(mapProvider.MapData.SpawnPoints[i].Z);
            }
            stream.Write(memoryStream.ToArray());
        }
    }
}