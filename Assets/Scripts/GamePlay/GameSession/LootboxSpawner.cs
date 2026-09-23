using System;
using System.Collections.Generic;
using Data;
using R3;
using UnityEngine;
using VoxelMap;
using Random = UnityEngine.Random;

namespace GamePlay
{
    public class LootBoxSpawner
    {
        private static readonly LootBoxType[] LootBoxTypes = (LootBoxType[])Enum.GetValues(typeof(LootBoxType));
        private readonly HashSet<Vector2Int> _busyPositions = new HashSet<Vector2Int>();
        private float _boxSpawnTimer;
        private float _boxRespawnTime;

        private readonly IEntityFactory _entityFactory;
        private readonly MapProvider _mapProvider;

        private bool _initialized;

        public LootBoxSpawner(IEntityFactory entityFactory, MapProvider mapProvider)
        {
            _entityFactory = entityFactory;
            _mapProvider = mapProvider;
        }

        public void SetBoxRespawnTime(TimeSpan time)
        {
            _boxRespawnTime = (float)time.TotalSeconds;
            _initialized = true;
        }

        public void OnUpdate(float deltaTime)
        {
            if (!_initialized)
            {
                return;
            }

            _boxSpawnTimer += deltaTime;

            var map = _mapProvider.Map.CurrentValue;

            if (map == null)
            {
                return;
            }

            if (_boxSpawnTimer < _boxRespawnTime ||
                !map.TryGetRandomTopVoxelPosition(out Vector3Ushort topVoxelPosition) ||
                topVoxelPosition.y == 0 ||
                _busyPositions.Contains(ToGrid(topVoxelPosition)))
            {
                return;
            }

            SpawnLootBox(map, ToGrid(topVoxelPosition), topVoxelPosition);

            _boxSpawnTimer = 0;
        }

        public void Clean()
        {
            _busyPositions.Clear();
            _boxSpawnTimer = 0;
        }

        private void SpawnLootBox(Map map, Vector2Int gridPosition, Vector3Ushort topVoxelPosition)
        {
            _busyPositions.Add(gridPosition);
            var boxPosition = new Vector3(topVoxelPosition.x + Map.WorldOffset.x, map.Height - 1,
                topVoxelPosition.z + Map.WorldOffset.z);
            var lootBoxType = LootBoxTypes[Random.Range(0, LootBoxTypes.Length)];
            var lootBox = _entityFactory.CreateLootBox(lootBoxType, boxPosition);
            lootBox.PickedUp.Subscribe(_ => _busyPositions.Remove(gridPosition)).AddTo(lootBox);
        }

        private static Vector2Int ToGrid(Vector3Ushort v) => new Vector2Int(v.x, v.z);
    }
}