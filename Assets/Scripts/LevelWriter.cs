using System;
using System.IO;

public static class LevelWriter
{
    public static void SaveLevel(string fileName, Block[] blocks)
    {
        if (Path.GetExtension(fileName) != ".rch")
        {
            throw new ArgumentException();
        }

        var strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var strWorkPath = Path.GetDirectoryName(strExeFilePath) + @"\..\..\Assets\Maps\";
        using var binaryWriter = new BinaryWriter(File.OpenWrite(strWorkPath + fileName));
        for (var i = 0; i < Level.Width; i++)
        {
            for (var j = 0; j < Level.Height; j++)
            {
                for (var k = 0; k < Level.Depth; k++)
                {
                    binaryWriter.Write(blocks[i * Level.Height * Level.Depth + j * Level.Depth + k].color);
                    binaryWriter.Write(blocks[i * Level.Height * Level.Depth + j * Level.Depth + k].isInvincible);
                }
            }
        }
    }
}