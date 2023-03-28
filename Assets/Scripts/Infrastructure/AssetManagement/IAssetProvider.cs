using Infrastructure.Services;
using UnityEngine;

namespace Infrastructure.AssetManagement
{
    public interface IAssetProvider : IService
    {
        public GameObject Instantiate(string path, Transform transform);
        public GameObject Instantiate(string path, Vector3 position, Quaternion rotation);
    }
}