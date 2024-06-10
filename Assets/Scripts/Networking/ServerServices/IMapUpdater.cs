using System;
using System.Collections.Generic;
using Data;

namespace Networking.ServerServices
{
    public interface IMapUpdater
    {
        event Action MapUpdated;
        void UpdateMap(List<BlockDataWithPosition> changedBlocks);
    }
}