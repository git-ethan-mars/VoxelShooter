using System.IO;

public static class MapReader
{
    public static Map CreateNewMap(string fileName, int width = 512, int height = 64, int depth = 512)
    {
        var chunks = new ChunkData[width / ChunkData.ChunkSize * height / ChunkData.ChunkSize * depth /
                                   ChunkData.ChunkSize];
        for (var i = 0; i < chunks.Length; i++)
        {
            chunks[i] = new ChunkData();
        }

        return new Map(chunks, width, height, depth);
    }

    public static Map Read(string fileName)
    {
        var strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var filePath = Path.GetDirectoryName(strExeFilePath) + @"\..\..\Assets\Maps\" + fileName;
        if (!File.Exists(filePath))
        {
            return CreateNewMap(fileName);
        }

        using var binaryReader = new BinaryReader(File.OpenRead(filePath));
        var width = binaryReader.ReadInt32();
        var height = binaryReader.ReadInt32();
        var depth = binaryReader.ReadInt32();
        var chunks = new ChunkData[width / ChunkData.ChunkSize * height / ChunkData.ChunkSize * depth /
                                   ChunkData.ChunkSize];
        var map = new Map(chunks, width, height, depth);
        for (var x = 0; x < width / ChunkData.ChunkSize; x++)
        {
            for (var y = 0; y < height / ChunkData.ChunkSize; y++)
            {
                for (var z = 0; z < depth / ChunkData.ChunkSize; z++)
                {
                    chunks[
                        z + y * (depth / ChunkData.ChunkSize) +
                        x * (height / ChunkData.ChunkSize * depth / ChunkData.ChunkSize)] = ReadChunk();
                }
            }
        }

        return map;

        ChunkData ReadChunk()
        {
            var chunk = new ChunkData();
            for (var x = 0; x < ChunkData.ChunkSize; x++)
            {
                for (var y = 0; y < ChunkData.ChunkSize; y++)
                {
                    for (var z = 0; z < ChunkData.ChunkSize; z++)
                    {
                        var blockKind = (BlockKind) binaryReader.ReadByte();
                        var block = new Block()
                        {
                            Kind = blockKind
                        };
                        chunk.Blocks[x, y, z] = block;
                    }
                }
            }

            return chunk;
        }
    }
}