using CustomAttributes;
using UnityEngine;

[CreateAssetMenu]
public class MapConfigure : ScriptableObject
{
    [ReadOnly]
    public string mapName;

    [Header("Lighting")]
    [ReadOnly]
    public Vector3 lightPosition;

    [ReadOnly]
    public Quaternion lightRotation;

    [ReadOnly]
    public Color lightColor;

    [Header("Skybox")]
    [ReadOnly]
    public Material skyboxMaterial;

    private void OnValidate()
    {
        mapName = name;
    }
}