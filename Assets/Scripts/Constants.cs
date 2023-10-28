using UnityEngine;

public static class Constants
{
    public static readonly string mapFolderPath = $"{Application.dataPath}/Maps";
    public static readonly Vector3 worldOffset = new Vector3(0.5f, 0.5f, 0.5f);
    public static readonly LayerMask attackMask = LayerMask.GetMask("Body") | LayerMask.GetMask("Chunk");
    public static readonly LayerMask buildMask = LayerMask.GetMask("Chunk");
    public const string VxlExtension = ".vxl";
    public const string RchExtension = ".rch";
    public const float Epsilon = 0.001f;
    public const int DefaultFov = 60;
    public const int MessageSize = 500 * 1024;
    public const float MessageDelay = 1f;
    public static readonly bool isLocalBuild;
}