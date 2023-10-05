using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Blueprints
{
    public static class BlueprintSaver
    {
        private const string BlueprintExtension = ".bch";
        private static readonly string FolderPath = Application.dataPath + $"/Blueprints";

        public static void Save(string name, Dictionary<Vector3Int, Color32> blueprintData)
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            using var fileStream = File.OpenWrite($"{FolderPath}/{name}{BlueprintExtension}");
            using var binaryWriter = new BinaryWriter(fileStream);
            binaryWriter.Write(blueprintData.Count);
            foreach (var (position, color) in blueprintData)
            {
                binaryWriter.Write(position.x);
                binaryWriter.Write(position.y);
                binaryWriter.Write(position.z);
                binaryWriter.Write(color.b);
                binaryWriter.Write(color.g);
                binaryWriter.Write(color.r);
                binaryWriter.Write(color.a);
            }
        }
    }
}