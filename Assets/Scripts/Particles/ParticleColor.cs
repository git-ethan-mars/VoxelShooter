using Mirror;
using UnityEngine;

public class ParticleColor : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnColorChanged))] public Color color;

    public void OnColorChanged(Color oldColor, Color newColor)
    {
        var main = GetComponent<ParticleSystem>().main;
        main.startColor = newColor;
    }
}
