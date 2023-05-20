﻿using Infrastructure.AssetManagement;
using Mirror;
using UnityEngine;

namespace Infrastructure.Factory
{
    public class EntityFactory : IEntityFactory
    {
        private readonly IAssetProvider _assets;
        private const string TntPath = "Prefabs/Tnt";

        public EntityFactory(IAssetProvider assets)
        {
            _assets = assets;
        }
        public GameObject CreateTnt(Vector3 position, Quaternion rotation)
        {
            var tnt = _assets.Instantiate(TntPath, position, rotation);
            NetworkServer.Spawn(tnt);
            return tnt;
        }
    }
}