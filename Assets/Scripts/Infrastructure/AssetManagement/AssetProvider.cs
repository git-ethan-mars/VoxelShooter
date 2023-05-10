using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Infrastructure.AssetManagement
{
    public class AssetProvider : IAssetProvider
    {
        private static readonly string ResourcesPath = Application.dataPath + "/Resources/";

        public T Load<T>(string path) where T : Object
        {
            var obj = Resources.Load<T>(path);
            return obj;
        }

        public List<T> LoadAllInSubdirectories<T>(string path) where T : Object
        {
            var directories = Directory.GetDirectories(ResourcesPath + path, "*", SearchOption.AllDirectories);
            var resources = new List<T>();
            foreach (var directory in directories)
            {
                var localPath = path + "/" + directory.Substring((ResourcesPath + path).Length + 1);
                resources.AddRange(Resources.LoadAll<T>(localPath));
            }

            return resources;
        }

        public GameObject Instantiate(string path)
        {
            var prefab = Resources.Load<GameObject>(path);
            return Object.Instantiate(prefab);
        }

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

        public GameObject Instantiate(string path, Vector3 position, Quaternion rotation, Transform parent)
        {
            var prefab = Resources.Load<GameObject>(path);
            return Object.Instantiate(prefab, position, rotation, parent);
        }
    }
}