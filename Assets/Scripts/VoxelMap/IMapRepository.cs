using System;
using Common;

namespace VoxelMap
{
    public interface IMapRepository : IService
    {
        Tuple<string, MapConfigure> GetCurrentMap();
        Tuple<string, MapConfigure> GetNextMap();
        Tuple<string, MapConfigure> GetPreviousMap();
    }
}