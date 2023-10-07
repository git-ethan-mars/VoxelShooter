using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Infrastructure.Services.StaticData;
using UnityEngine;

public class MapRepository : IMapRepository
{
    private const string MapExtension = ".rch";
    private static readonly string FolderPath = Application.dataPath + $"/Maps";
    private readonly IStaticDataService _staticData;
    private readonly List<Tuple<string, MapConfigure>> _namedConfigures;
    private int _index;

    public MapRepository(IStaticDataService staticData)
    {
        _staticData = staticData;
        _namedConfigures = new List<Tuple<string, MapConfigure>>();
    }

    public Tuple<string, MapConfigure> GetCurrentMap()
    {
        UpdateRepository();
        if (_namedConfigures.Count == 0)
            return null;
        return _namedConfigures[_index];
    }

    public Tuple<string, MapConfigure> GetNextMap()
    {
        UpdateRepository();
        if (_namedConfigures.Count == 0)
            return null;
        _index = (_index + 1) % _namedConfigures.Count;
        return _namedConfigures[_index];
    }

    public Tuple<string, MapConfigure> GetPreviousMap()
    {
        UpdateRepository();
        if (_namedConfigures.Count == 0)
            return null;
        _index = (_index - 1) % _namedConfigures.Count;
        return _namedConfigures[_index];
    }

    private void UpdateRepository()
    {
        if (!Directory.Exists(FolderPath))
        {
            Directory.CreateDirectory(FolderPath);
        }

        var mapNames = Directory.GetFiles(FolderPath, $@"*{MapExtension}").Select(Path.GetFileNameWithoutExtension);
        _staticData.LoadMapConfigures();
        _namedConfigures.Clear();
        foreach (var mapName in mapNames)
        {
            _namedConfigures.Add(new Tuple<string, MapConfigure>(mapName, _staticData.GetMapConfigure(mapName)));
        }
    }
}