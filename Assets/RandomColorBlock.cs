using UnityEngine;

public class RandomColorBlock : MonoBehaviour
{
    private MeshRenderer _meshRenderer;

    private void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.material.color = new Color(Random.value, Random.value, Random.value);
    }
}
