using UnityEngine;

namespace GamePlay.Entities
{
    public interface IPushable
    {
        public Vector3Int Center { get; }
        public Vector3Int Min { get; }
        public Vector3Int Max { get; }
        public void Push();
        public void Fall();
    }
}