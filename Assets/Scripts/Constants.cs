﻿using UnityEngine;

public static class Constants
{
    public static readonly string mapFolderPath = $"{Application.dataPath}/Maps";
    public static Vector3 WorldOffset = new Vector3(0.5f, 0.5f, 0.5f);
    public static LayerMask AttackMask = LayerMask.GetMask("Body") | LayerMask.GetMask("Chunk");
    public static LayerMask BuildMask = LayerMask.GetMask("Chunk");
    public const string VxlExtension = ".vxl";
    public const string RchExtension = ".rch";
    public const float Epsilon = 0.001f;
    public const int DefaultFov = 60;
    public const int MessageSize = 100000;
    public const float MessageDelay = 1f;
}