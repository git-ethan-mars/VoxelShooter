using System.Collections.Generic;
using Infrastructure.AssetManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rendering
{
    
    public class TransparentMeshPool
    {
        private readonly Material _transparentMaterial;
        private readonly List<GameObject> _pool;

        public TransparentMeshPool(IAssetProvider assets)
        {
            _transparentMaterial = assets.Load<Material>("Materials/Transparent");
            _pool = new List<GameObject>();
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
            gameObject.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            gameObject.transform.localScale = prefab.transform.localScale;
            _pool.Add(gameObject);
            return gameObject;
        }

        public void CleanPool()
        {
            for (var i = 0; i < _pool.Count; i++)
            {
                Object.Destroy(_pool[i]);
            }
        }
    }
}