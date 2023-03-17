using System.Collections.Generic;
using Core;
using UnityEngine;

namespace GamePlay
{
    public class Explosion : MonoBehaviour
    {
        [SerializeField] private float explosionForce;
        [SerializeField] private int explosionRadius;
        [SerializeField] private GameObject slicedBlock;

        public void Start()
        {
            var explosionCenter = transform.position;
            var globalPositions = new List<Vector3Int>();
            for (var x = -explosionRadius; x < explosionRadius; x++)
            {
                for (var y = -explosionRadius; y < explosionRadius; y++)
                {
                    for (var z = -explosionRadius; z < explosionRadius; z++)
                    {
                        globalPositions.Add(new Vector3Int((int) explosionCenter.x,
                            (int) explosionCenter.y, (int) explosionCenter.z) + new Vector3Int(x, y, z));
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

            GlobalEvents.SendBlockStates(validPositions, new Block[validPositions.Count]);
        }
    }
}