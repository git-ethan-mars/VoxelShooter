using UnityEngine;

namespace Data
{
    public struct SpawnPoint
    {
        public int X;
        public int Y;
        public int Z;

        public Vector3 ToUnityVector()
        {
            return new Vector3(X, Y, Z);
        }
    }
}