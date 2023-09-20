using System.Collections.Generic;
using CustomAttributes;
using Data;
using UnityEngine;

[CreateAssetMenu]
public class MapConfigure : ScriptableObject
{
    [ReadOnly]
    public string mapName;

    [Header("Color")]
    [ReadOnly]
    public Color32 waterColor = new(3, 58, 135, 255);

    [ReadOnly]
    public Color32 innerColor = new(89, 53, 47, 255);

    [Header("Lighting")]
    public LightData lightData = new(Vector3.zero, Quaternion.identity, Color.white);

    [Header("Skybox")]
    [ReadOnly]
    public Material skyboxMaterial;

    [Header("Spawn points")]
    public List<SpawnPointData> spawnPoints = new();
}