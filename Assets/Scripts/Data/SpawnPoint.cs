using UnityEngine;

namespace Data
{
    public struct SpawnPoint
    {
        public int X;
        public int Y;
        public int Z;

        public SpawnPoint(Vector3Int position)
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