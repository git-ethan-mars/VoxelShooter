using System.IO;
using Infrastructure.AssetManagement;
using Infrastructure.Services.StaticData;
using MapLogic;
using UnityEditor;

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
                var mapProvider = Vxl2RchConverter.LoadVxl(vxlPath, staticData.GetMapConfigure(fileName));
                MapWriter.SaveMap($"{fileName}{Constants.RchExtension}", mapProvider);
            }
        }
    }
}