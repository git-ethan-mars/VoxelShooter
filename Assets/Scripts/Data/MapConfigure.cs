using System.Collections.Generic;
using CustomAttributes;
using Data;
using UnityEngine;
using UnityEngine.Rendering;

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

    [Header("Ambient light")]
    [ReadOnly]
    public AmbientMode ambientMode;

    [ReadOnly]
    public Color skyColor;

    [ReadOnly]
    public Color equatorColor;

    [ReadOnly]
    public Color groundColor;

    [ReadOnly]
    public float ambientIntensity;

    [Header("Fog")]
    [ReadOnly]
    public bool isFogActivated;

    [ReadOnly]
    public FogMode fogMode;

    [ReadOnly]
    public Color fogColor;

    [ReadOnly]
    public float fogStartDistance;

    [ReadOnly]
    public float fogEndDistance;

    [ReadOnly]
    public float fogDensity = 1;

    [Header("Spawn points")]
    public List<SpawnPointData> spawnPoints = new();
}