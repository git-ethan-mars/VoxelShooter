using UnityEngine;

public static class Constants
{
    public static LayerMask AttackMask = LayerMask.GetMask("Player") | LayerMask.GetMask("Chunk");
    public static LayerMask BuildMask = LayerMask.GetMask("Chunk");
    public const float Epsilon = 0.001f;
    public const int DefaultFov = 60;
}