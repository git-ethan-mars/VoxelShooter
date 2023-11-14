using System;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class SpawnPointData
    {
        public Vector3Int position;

        public SpawnPointData(Vector3Int position)
        {
            this.position = position;
        }

        public Vector3 ToVectorWithOffset()
        {
            return position + Constants.worldOffset;
        }
    }
}