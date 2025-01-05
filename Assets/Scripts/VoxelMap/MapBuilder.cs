using Common.AssetManagement;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace VoxelMap
{
    public class MapBuilder
    {
        private readonly IMapFactory _mapFactory;

        private Color32 _waterColor = VoxelData.Air.Color;
        private Color32 _innerColor;
        private bool _waterColorChanging;
        private bool _innerColorChanging;
        private bool _enableWalls;
        private FogData _fog;
        private LightData _light;
        private Material _skybox;
        private AmbientData _ambient;

        public MapBuilder(IAssetProvider assets)
        {
            _mapFactory = new MapFactory(assets);
        }

        public MapBuilder WithWaterColor(Color32 color)
        {
            _waterColorChanging = true;
            _waterColor = color;
            return this;
        }

        public MapBuilder WithInnerColor(Color32 color)
        {
            _innerColorChanging = true;
            _innerColor = color;
            return this;
        }

        public MapBuilder WithDirectionalLight(LightData lightData)
        {
            _light = lightData;
            return this;
        }
        public MapBuilder WithFog(FogData fogData)
        {
            _fog = fogData;
            return this;
        }

        public MapBuilder WithSkybox(Material skybox)
        {
            _skybox = skybox;
            return this;
        }

        public MapBuilder WithAmbient(AmbientData ambient)
        {
            _ambient = ambient;
            return this;
        }
        public MapBuilder WithWalls()
        {
            _enableWalls = true;
            return this;
        }

        public async UniTask<Map> BuildAsync(MapData mapData, Transform container = null)
        {
            var map = _mapFactory.CreateMap(mapData, container);
            var mapGenerator = new MapGenerator(_mapFactory, mapData);
            if (_innerColorChanging)
            {
                SetInnerColor(mapData);
            }

            if (_waterColorChanging)
            {
                SetWaterColor(mapData);
                var mapCenter = new Vector3((float)mapData.Width / 2, 1, (float)mapData.Depth / 2);
                var waterPlane = _mapFactory.CreateWaterPlane(mapCenter, _waterColor, map.transform);
                waterPlane.GetComponent<MeshRenderer>().sharedMaterial.color = _waterColor;
            }

            await UniTask.Yield();

            var chunks = mapGenerator.GenerateChunks(map.transform);
            map.SetChunks(chunks);
            if (_light is not null)
            {
                var directionalLight = _mapFactory.CreateDirectionalLight(_light, map.transform);
                map.SetDirectionalLight(directionalLight);
            }

            if (_enableWalls)
            {
                _mapFactory.CreateWalls(mapData, map.transform);
            }

            if (_fog != null)
            {
                Environment.ApplyFog(_fog);
            }

            if (_skybox != null)
            {
                Environment.ApplySkybox(_skybox);
            }

            if (_ambient != null)
            {
                Environment.ApplyAmbientLighting(_ambient);
            }

            return map;
        }

        private void SetWaterColor(MapData data)
        {
            var lowerChunks = new ChunkData[data.Width * data.Depth / ChunkData.ChunkSizeSquared];
            for (var i = 0; i < data.Width / ChunkData.ChunkSize; i++)
            {
                for (var j = 0; j < data.Depth / ChunkData.ChunkSize; j++)
                {
                    lowerChunks[i * data.Depth / ChunkData.ChunkSize + j] = data.GetChunkDataByIndex(data.Depth * data.Height / ChunkData
                        .ChunkSizeSquared * i + j);
                }
            }

            var jobHandles = new NativeArray<JobHandle>(lowerChunks.Length, Allocator.TempJob);
            
            for (var i = 0; i < lowerChunks.Length; i++)
            {
                jobHandles[i] = new WaterColorChangeJob()
                {
                    Voxels = lowerChunks[i].Voxels,
                    WaterColor = _waterColor
                }.Schedule();
            }
            
            JobHandle.CompleteAll(jobHandles);
            jobHandles.Dispose();
        }

        private void SetInnerColor(MapData mapData)
        {
            var jobHandles = new NativeArray<JobHandle>(mapData.ChunkCount, Allocator.TempJob);
            for (var i = 0; i < mapData.ChunkCount; i++)
            {
                jobHandles[i] = new InnerColorChangeJob()
                {
                    Voxels = mapData.GetChunkDataByIndex(i).Voxels,
                    NewInnerColor = _innerColor,
                }.Schedule();
            }

            JobHandle.CompleteAll(jobHandles);
            jobHandles.Dispose();
        }
    }
}