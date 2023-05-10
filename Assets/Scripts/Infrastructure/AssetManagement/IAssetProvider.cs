using System.Collections.Generic;
using Infrastructure.Services;
using UnityEngine;

namespace Infrastructure.AssetManagement
{
    public interface IAssetProvider : IService
    {
        public GameObject Instantiate(string path);
        public GameObject Instantiate(string path, Transform parent);
        public GameObject Instantiate(string path, Vector3 position, Quaternion rotation);

        public GameObject Instantiate(string path, Vector3 position, Quaternion rotation, Transform parent);
        T Load<T>(string path) where T : Object;
        List<T> LoadAllInSubdirectories<T>(string path) where T : Object;
    }
}