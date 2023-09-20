using System;
using System.Collections.Generic;
using System.IO;
using CustomAttributes;
using Data;
using UnityEditor;
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

    private void Reset()
    {
        FetchMapName();
    }

    private void OnValidate()
    {
        FetchMapName();
    }

    private void FetchMapName()
    {
        var assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
        mapName = Path.GetFileNameWithoutExtension(assetPath);
    }
}