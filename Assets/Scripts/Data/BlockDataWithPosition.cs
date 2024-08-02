using System;
using UnityEngine;

namespace Data
{
    [Serializable]
    public struct BlockDataWithPosition
    {
        public Vector3Int Position;
        public BlockData BlockData;

        public BlockDataWithPosition(Vector3Int position, BlockData blockData)
        {
            Position = position;
            BlockData = blockData;
        }
    }
}