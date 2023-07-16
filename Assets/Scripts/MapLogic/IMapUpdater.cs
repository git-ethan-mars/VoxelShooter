﻿using System;
using Data;
using UnityEngine;

namespace MapLogic
{
    public interface IMapUpdater
    {
        event Action<Vector3Int, BlockData> MapUpdated;
        void SetBlockByGlobalPosition(Vector3Int position, BlockData blockData);
    }
}