﻿using Infrastructure.Services;
using UnityEngine;

namespace Infrastructure.AssetManagement
{
    public interface IAssetProvider : IService
    {
        public GameObject Instantiate(string path);
        public GameObject Instantiate(string path, Transform parent);
        public GameObject Instantiate(string path, Vector3 position, Quaternion rotation);

        public GameObject Instantiate(string path, Vector3 position, Quaternion rotation, Transform parent);
    }
}