using System.IO;
using Common;
using Common.AssetManagement;
using Common.Services.StaticData;
using Common.StaticData;
using MapLogic;
using UnityEditor;
using VoxelMap;

namespace Editor
{
    public static class Vxl2RchAutomation
    {
        [MenuItem("Vxl2RchConverter/Convert all")]
        public static void ConvertAll()
        {
            var staticData = new StaticDataService(new AssetProvider());
            staticData.LoadMapConfigures();
            var mapFolder = Path.Combine(Constants.mapFolderPath);
            var vxlFilePaths = Directory.GetFiles(mapFolder, $"*{Constants.VxlExtension}");
            foreach (var vxlPath in vxlFilePaths)
            {
                var fileName = Path.GetFileNameWithoutExtension(Path.GetFileName(vxlPath));
                var mapData = Vxl2RchConverter.LoadVxl(vxlPath);
                MapWriter.SaveMap($"{fileName}{Constants.RchExtension}", mapData);
            }
        }
    }
}