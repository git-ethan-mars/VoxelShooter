using UnityEngine;

namespace GamePlay
{
    public class RandomColorBlock : MonoBehaviour
    {
        private MeshRenderer MeshRenderer { get; set; }

        private void Start()
        {
            MeshRenderer = GetComponent<MeshRenderer>();
            MeshRenderer.material.color = new UnityEngine.Color(Random.value, Random.value, Random.value);
        }
    }
}
