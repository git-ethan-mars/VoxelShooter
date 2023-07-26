using UnityEngine;

namespace Data
{
    public struct SpawnPointData
    {
        public int X;
        public int Y;
        public int Z;

        public SpawnPointData(Vector3Int position)
        {
            X = position.x;
            Y = position.y;
            Z = position.z;
        }

        public Vector3 ToVectorWithOffset()
        {
            return new Vector3(X, Y, Z) + Constants.WorldOffset;
        }
    }
}