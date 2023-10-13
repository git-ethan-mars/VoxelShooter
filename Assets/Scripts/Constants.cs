using UnityEngine;

public static class Constants
{
    public static readonly string mapFolderPath = $"{Application.dataPath}/Maps";
    public static Vector3 WorldOffset = new(0.5f, 0.5f, 0.5f);
    public static LayerMask AttackMask = LayerMask.GetMask("Player") | LayerMask.GetMask("Chunk");
    public static LayerMask BuildMask = LayerMask.GetMask("Chunk");
    public const string VxlExtension = ".vxl";
    public const string RchExtension = ".rch";
    public const float Epsilon = 0.001f;
    public const int DefaultFov = 60;
}