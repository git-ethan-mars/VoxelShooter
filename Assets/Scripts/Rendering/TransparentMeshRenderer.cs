using UnityEngine;

namespace Rendering
{
    
    public class TransparentMeshRenderer
    {
        private readonly Material _transparentMaterial;

        public TransparentMeshRenderer()
        {
            _transparentMaterial = Resources.Load<Material>("Materials/Transparent");
        }

        public GameObject CreateTransparentGameObject(GameObject prefab, Color32 color)
        {
            var gameObject = new GameObject();
            gameObject.name = $"{prefab.name} - transparent";
            gameObject.AddComponent<MeshFilter>().mesh = prefab.GetComponent<MeshFilter>().sharedMesh;
            var material = new Material(_transparentMaterial)
            {
                color = color
            };
            gameObject.AddComponent<MeshRenderer>().material = material;
            gameObject.transform.localScale = prefab.transform.localScale;
            return gameObject;
        }
        
    }
}