using UnityEngine;

namespace Infrastructure.AssetManagement
{
    public class AssetProvider : IAssetProvider
    {
        public T Load<T>(string path) where T : Object
        {
            var obj = Resources.Load<T>(path);
            return obj;
        }

        public T[] LoadAll<T>(string path) where T : Object
        {
            var objects = Resources.LoadAll<T>(path);
            return objects;
        }

        public GameObject Instantiate(GameObject prefab, Transform parent)
        {
            return Object.Instantiate(prefab, parent);
        }

        public GameObject Instantiate(string path)
        {
            var prefab = Resources.Load<GameObject>(path);
            return Object.Instantiate(prefab);
        }

        public GameObject Instantiate(string path, Transform transform)
        {
            var prefab = Resources.Load<GameObject>(path);
            return Object.Instantiate(prefab, transform);
        }

        public GameObject Instantiate(string path, Vector3 position, Quaternion rotation)
        {
            var prefab = Resources.Load<GameObject>(path);
            return Object.Instantiate(prefab, position, rotation);
        }

        public GameObject Instantiate(string path, Vector3 position, Quaternion rotation, Transform parent)
        {
            var prefab = Resources.Load<GameObject>(path);
            return Object.Instantiate(prefab, position, rotation, parent);
        }
    }
}