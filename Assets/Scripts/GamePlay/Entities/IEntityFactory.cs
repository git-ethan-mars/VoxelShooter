using Common;
using GamePlay.Data;
using UnityEngine;
using VoxelMap;

namespace GamePlay.Entities
{
    public interface IEntityFactory : IService
    {
        SpawningTnt CreateSpawningTnt(Vector3 position, Quaternion rotation, TntData data);
        SpawningGrenade CreateSpawningGrenade(Vector3 position);
        Tombstone CreateTombstone(Vector3 position);
        Rocket CreateRocket(Vector3 position, Quaternion rotation, RocketLauncherData data, MapProvider mapProvider);
        LootBox CreateAmmoBox(Vector3 position, Transform parent);
        LootBox CreateHealthBox(Vector3 position, Transform parent);
        LootBox CreateBlockBox(Vector3 position, Transform parent);
        SpawnPoint CreateSpawnPoint(SpawnPointData position, Transform parent);
        Drill CreateDrill(Vector3 position, Quaternion rotation, DrillLauncherData data);
    }
}