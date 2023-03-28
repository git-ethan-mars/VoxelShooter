using UnityEngine;

namespace Infrastructure.AssetManagement
{
    public class AssetProvider : IAssetProvider
    {
        public GameObject Instantiate(string path, Transform transform)
        {
            var prefab = Resources.Load<GameObject>(path);
            return Object.Instantiate(prefab, transform.position, transform.rotation);
        }

        public GameObject Instantiate(string path, Vector3 position, Quaternion rotation)
        {
            var prefab = Resources.Load<GameObject>(path);
            return Object.Instantiate(prefab, position, rotation);
        }
    }
}