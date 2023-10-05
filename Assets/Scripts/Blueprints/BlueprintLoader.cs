using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data;
using UnityEngine;

namespace Blueprints
{
    public static class BlueprintLoader
    {
        private const string BlueprintExtension = ".bch";
        private static readonly string FolderPath = Application.dataPath + $"/Blueprints";

        public static IEnumerable<string> LoadBlueprintFiles()
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            return Directory.GetFiles(FolderPath, $@"*{BlueprintExtension}").Select(Path.GetFileNameWithoutExtension);
        }

        public static Dictionary<Vector3Int, Color32> GetBlueprintData(string name)
        {
            var blueprintData = new Dictionary<Vector3Int, Color32>();

            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            if (!File.Exists($"{FolderPath}/{name}{BlueprintExtension}"))
            {
                File.Create($"{FolderPath}/{name}{BlueprintExtension}");
                return blueprintData;
            }

            using var fileStream = File.OpenRead($"{FolderPath}/{name}{BlueprintExtension}");
            fileStream.Seek(0, SeekOrigin.Begin);
            using var binaryReader = new BinaryReader(fileStream);
            var blockCount = binaryReader.ReadInt32();
            for (var i = 0; i < blockCount; i++)
            {
                var x = binaryReader.ReadInt32();
                var y = binaryReader.ReadInt32();
                var z = binaryReader.ReadInt32();
                var color = BlockColor.UInt32ToColor(binaryReader.ReadUInt32());
                blueprintData[new Vector3Int(x, y, z)] = color;
            }

            return blueprintData;
        }
    }
}