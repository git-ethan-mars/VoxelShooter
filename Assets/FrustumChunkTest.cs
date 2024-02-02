using UnityEngine;

public class FrustumChunkTest : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer meshRenderer;

    [SerializeField]
    private Material green;

    [SerializeField]
    private Material red;

    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
        meshRenderer.material = red;
    }

    private void Update()
    {
        meshRenderer.material = red;
    }

    private void OnWillRenderObject()
    {
        if (_camera == Camera.current)
        {
            meshRenderer.material = green;
        }
    }
}