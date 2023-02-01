using System.IO;

public class LevelReader
{
    public static Block[] ReadLevel(string fileName)
    {
        var blocks = new Block[Level.Width * Level.Height * Level.Depth];
        var strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var strWorkPath = Path.GetDirectoryName(strExeFilePath) + @"\..\..\Assets\Maps\";
        using var binaryReader = new BinaryReader(File.OpenRead(strWorkPath + fileName));
        for (var i = 0; i < Level.Width; i++)
        {
            for (var j = 0; j < Level.Height; j++)
            {
                for (var k = 0; k < Level.Depth; k++)
                {
                    var block = new Block
                    {
                        color = binaryReader.ReadByte(),
                        isInvincible = binaryReader.ReadBoolean()
                    };
                    blocks[i * Level.Height * Level.Depth + j * Level.Depth + k] = block;
                }
            }
        }

        return blocks;
    }
}