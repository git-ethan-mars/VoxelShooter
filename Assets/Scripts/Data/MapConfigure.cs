using System.Collections.Generic;
using CustomAttributes;
using Data;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu]
public class MapConfigure : ScriptableObject
{
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
    public Color skyColor = new Color32(54, 58, 66, 255);

    [ReadOnly]
    public Color equatorColor = new Color32(29, 32, 34, 255);

    [ReadOnly]
    public Color groundColor = new Color32(12, 11, 9, 255);

    [ReadOnly]
    public float ambientIntensity = 1;

    [Header("Fog")]
    [ReadOnly]
    public bool isFogActivated;

    [ReadOnly]
    public FogMode fogMode = FogMode.Linear;

    [ReadOnly]
    public Color fogColor = new(0.5f, 0.5f, 0.5f, 1);

    [ReadOnly]
    public float fogStartDistance;

    [ReadOnly]
    public float fogEndDistance = 300;

    [ReadOnly]
    public float fogDensity = 0.01f;

    [Header("Spawn points")]
    public List<SpawnPointData> spawnPoints = new();
}