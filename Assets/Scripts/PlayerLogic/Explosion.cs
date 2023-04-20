using System.Collections.Generic;
using Data;
using Networking.Synchronization;
using UnityEngine;

namespace PlayerLogic
{
    public class Explosion : MonoBehaviour
    {
        [SerializeField] private int explosionRadius;
        private MapSynchronization _mapSynchronization;

        public void Construct(MapSynchronization mapSynchronization)
        {
            _mapSynchronization = mapSynchronization;
        }
        
        public void Start()
        {
            var gameObjectPosition = transform.position;
            var explosionCenter = new Vector3Int((int)gameObjectPosition.x, (int)gameObjectPosition.y, (int)gameObjectPosition.z);
            var globalPositions = new List<Vector3Int>();
            for (var x = -explosionRadius; x < explosionRadius; x++)
            {
                for (var y = -explosionRadius; y < explosionRadius; y++)
                {
                    for (var z = -explosionRadius; z < explosionRadius; z++)
                    {
                        globalPositions.Add(new Vector3Int(explosionCenter.x,
                            explosionCenter.y, explosionCenter.z) + new Vector3Int(x, y, z));
                    }
                }
            }

            var validPositions = new List<Vector3Int>();
            foreach (var blockPosition in globalPositions)
            {
                if ((blockPosition.x - explosionCenter.x) * (blockPosition.x - explosionCenter.x)
                    + (blockPosition.y - explosionCenter.y) * (blockPosition.y - explosionCenter.y)
                    + (blockPosition.z - explosionCenter.z) * (blockPosition.z - explosionCenter.z) <=
                    explosionRadius * explosionRadius)
                    validPositions.Add(blockPosition);
            }

            _mapSynchronization.UpdateBlocksOnServer(validPositions.ToArray(), new BlockData[validPositions.Count]);
        }
    }
}