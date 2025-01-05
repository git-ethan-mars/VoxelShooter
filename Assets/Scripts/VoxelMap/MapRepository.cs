using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common;

namespace VoxelMap
{
    public class MapRepository : IMapRepository
    {
        private readonly IMapConfigureLoader _mapConfigureLoader;
        private readonly List<Tuple<string, MapConfigure>> _namedConfigures;
        private int _index;

        public MapRepository(IMapConfigureLoader mapConfigureLoader)
        {
            _mapConfigureLoader = mapConfigureLoader;
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
            _index = (_namedConfigures.Count + _index + 1) % _namedConfigures.Count;
            return _namedConfigures[_index];
        }

        public Tuple<string, MapConfigure> GetPreviousMap()
        {
            UpdateRepository();
            if (_namedConfigures.Count == 0)
                return null;
            _index = (_namedConfigures.Count + _index - 1) % _namedConfigures.Count;
            return _namedConfigures[_index];
        }

        private void UpdateRepository()
        {
            if (!Directory.Exists(Constants.MapFolderPath))
            {
                Directory.CreateDirectory(Constants.MapFolderPath);
            }

            var mapNames = Directory.GetFiles(Constants.MapFolderPath, $"*{Constants.RchExtension}")
                .Union(Directory.GetFiles(Constants.MapFolderPath, $"*{Constants.VxlExtension}"))
                .Select(Path.GetFileNameWithoutExtension);
            _namedConfigures.Clear();
            foreach (var mapName in mapNames)
            {
                _namedConfigures.Add(new Tuple<string, MapConfigure>(mapName, _mapConfigureLoader.GetMapConfigure(mapName)));
            }
        }
    }
}