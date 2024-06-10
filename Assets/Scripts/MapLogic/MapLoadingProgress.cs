using System;
using System.Collections.Generic;
using System.IO;
using Infrastructure;
using Infrastructure.Services.StaticData;

namespace MapLogic
{
    public class MapLoadingProgress
    {
        public Action<MapProvider> MapLoaded;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        public bool IsMapLoaded => LoadProgress.Value == 1;
        public ObservableVariable<float> LoadProgress { get; } = new(0);
        public string MapName { set; get; }

        private readonly List<byte> _buffer = new();
        private readonly IStaticDataService _staticData;

        public MapLoadingProgress(IStaticDataService staticData)
        {
            _staticData = staticData;
        }

        public void AddBytes(byte[] bytes)
        {
            _buffer.AddRange(bytes);
        }

        public void UpdateProgress(float newProgress)
        {
            LoadProgress.Value = newProgress;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (IsMapLoaded)
            {
                var mapConfigure = _staticData.GetMapConfigure(MapName);
                var mapData = MapReader.ReadFromStream(new MemoryStream(_buffer.ToArray()), mapConfigure);
                var mapProvider = new MapProvider(mapData);
                MapLoaded.Invoke(mapProvider);
            }
        }
    }
}