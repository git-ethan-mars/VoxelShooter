using System;
using Data;

namespace Infrastructure.Services.StaticData
{
    public interface IMapRepository : IService
    {
        Tuple<string, MapConfigure> GetCurrentMap();
        Tuple<string, MapConfigure> GetNextMap();
        Tuple<string, MapConfigure> GetPreviousMap();
    }
}