using System.IO;
using UnityEditor;
using VoxelMap;

namespace Editor
{
    public static class Vxl2RchAutomation
    {
        [MenuItem("Vxl2RchConverter/Convert all")]
        public static void ConvertAll()
        {
            var mapFolder = Path.Combine(Constants.MapFolderPath);
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