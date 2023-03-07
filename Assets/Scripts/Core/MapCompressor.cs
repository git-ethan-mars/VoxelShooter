using System.IO;
using System.IO.Compression;

namespace Core
{
    public static class MapCompressor
    {
        public static byte[] Compress(Map map)
        {
            var input = new MemoryStream();
            MapWriter.WriteMap(map, input);
            var data = input.ToArray();
            var output = new MemoryStream();
            using (var deflateStream = new DeflateStream(output, CompressionLevel.Optimal))
            {
                deflateStream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        public static Map Decompress(byte[] data)
        {
            var input = new MemoryStream(data);
            var output = new MemoryStream();
            using (var deflateStream = new DeflateStream(input, CompressionMode.Decompress))
            {
                deflateStream.CopyTo(output);
            }
            return MapReader.ReadFromStream(output);
        }
    }
}