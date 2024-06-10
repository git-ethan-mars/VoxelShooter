using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Explosions;
using Generators;
using Geometry;
using Infrastructure;
using Infrastructure.AssetManagement;
using Infrastructure.Factory;
using Infrastructure.Services;
using Infrastructure.Services.StaticData;
using MapLogic;
using Mirror;
using Optimization;
using Steamworks;
using Tests.EditMode;
using UnityEngine;
using UnityEngine.TestTools;

public class LagCompensationTests
{
    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    private readonly Ray _ray = new Ray(new Vector3(511, 200, 511), Vector3.down);
    private IMapProvider _mapProvider;


    [UnityTest]
    public IEnumerator LagCompensation()
    {
        RegisterDependencies();
        const string mapName = "Crossroads";
        var staticData = AllServices.Container.Single<IStaticDataService>();
        staticData.LoadMapConfigures();
        staticData.LoadBlockHealthBalance();
        staticData.LoadPlayerCharacteristics();
        staticData.LoadInventories();
        staticData.LoadItems();
        var mapProvider = MapReader.ReadFromFile(mapName, staticData);
        var gameFactory = AllServices.Container.Single<IGameFactory>();
        var meshFactory = AllServices.Container.Single<IMeshFactory>();
        var mapGenerator = new MapGenerator(mapProvider, gameFactory, meshFactory);
        var coroutineRunner = new GameObject("Main");
        coroutineRunner.gameObject.AddComponent<CoroutineRunner>();
        var chunkMeshes = mapGenerator.GenerateMap(coroutineRunner.transform);
        var chunkMeshUpdater = new MapMeshUpdater(chunkMeshes, mapProvider);
        var mapHistory = new MapHistory(coroutineRunner.GetComponent<ICoroutineRunner>(), mapProvider);
        var server = new MockServer(mapProvider, mapHistory, staticData, chunkMeshUpdater);
        server.Start();
        var connection = new LocalConnectionToClient();
        server.AddPlayer(connection, CSteamID.Nil, "Test");
        server.ChangeClass(connection, GameClass.Combatant);


        yield return new WaitForSeconds(NetworkServer.sendInterval);
        SimpleBenchmark.Execute(MyRaycast, mapProvider, mapHistory, 1000);
        SimpleBenchmark.Execute(MyRaycast, mapProvider, mapHistory, 2000);
        SimpleBenchmark.Execute(MyRaycast, mapProvider, mapHistory, 5000);
        SimpleBenchmark.Execute(MyRaycast, mapProvider, mapHistory, 10000);
        SimpleBenchmark.Execute(MyRaycast, mapProvider, mapHistory, 20000);
        SimpleBenchmark.Execute(UnityRaycast, 1000);
        SimpleBenchmark.Execute(UnityRaycast, 2000);
        SimpleBenchmark.Execute(UnityRaycast, 5000);
        SimpleBenchmark.Execute(UnityRaycast, 10000);
        SimpleBenchmark.Execute(UnityRaycast, 20000);
        server.Stop();
    }

    [UnityTest]
    public IEnumerator LagCompensationWithExplosions()
    {
        yield return new WaitForSeconds(5);
        RegisterDependencies();
        const string mapName = "Crossroads";
        var staticData = AllServices.Container.Single<IStaticDataService>();
        staticData.LoadMapConfigures();
        staticData.LoadBlockHealthBalance();
        staticData.LoadPlayerCharacteristics();
        staticData.LoadInventories();
        staticData.LoadItems();
        var mapProvider = MapReader.ReadFromFile(mapName, staticData);
        var gameFactory = AllServices.Container.Single<IGameFactory>();
        var meshFactory = AllServices.Container.Single<IMeshFactory>();
        var mapGenerator = new MapGenerator(mapProvider, gameFactory, meshFactory);
        var coroutineRunner = new GameObject("Main");
        coroutineRunner.gameObject.AddComponent<CoroutineRunner>();
        var chunkMeshes = mapGenerator.GenerateMap(coroutineRunner.transform);
        var chunkMeshUpdater = new MapMeshUpdater(chunkMeshes, mapProvider);
        var mapHistory = new MapHistory(coroutineRunner.GetComponent<ICoroutineRunner>(), mapProvider);
        var server = new MockServer(mapProvider, mapHistory, staticData, chunkMeshUpdater);
        server.Start();
        var connection = new LocalConnectionToClient();
        server.AddPlayer(connection, CSteamID.Nil, "Test");
        server.ChangeClass(connection, GameClass.Combatant);
        var explosionBehaviour = new ExplosionBehaviour(server, connection, 8, 250);
        var explosionPositions = new List<Vector3>()
        {
            new(120, 10, 133), new(133, 20, 146), new(147, 19, 190), new(215, 8, 168), new(261, 6, 204),
            new(277, 9, 294), new(288, 8, 219), new(296, 16, 301), new(304, 23, 303), new(349, 20, 334),
            new(339, 26, 344), new(332, 15, 384), new(310, 13, 378), new(299, 31, 364), new(228, 51, 231),
            new(164, 16, 171), new(170, 5, 251), new(206, 5, 256), new(320, 5, 253), new(372, 9, 184)
        };
        var sphereArea = new SphereBlockArea(mapProvider, 8);
        var explodedPositions = explosionPositions
            .SelectMany(position => sphereArea.GetOverlappedBlockPositions(Vector3Int.FloorToInt(position)))
            .ToList();
        for (var i = 0; i < explosionPositions.Count; i++)
        {
            explosionBehaviour.Explode(explosionPositions[i]);
        }

        yield return new WaitForSeconds(NetworkServer.sendInterval);
        SimpleBenchmark.Execute(MyRaycast, mapProvider, mapHistory);
        SimpleBenchmark.Execute(UnityRaycastWithChunkRollback, mapProvider, mapHistory, chunkMeshes, explodedPositions);
        explosionPositions = new List<Vector3>()
        {
            new(120, 10, 133), new(133, 20, 146), new(147, 19, 190), new(215, 8, 168), new(261, 6, 204),
            new(277, 9, 294), new(288, 8, 219), new(296, 16, 301), new(304, 23, 303), new(349, 20, 334),
        };
        explodedPositions = explosionPositions
            .SelectMany(position => sphereArea.GetOverlappedBlockPositions(Vector3Int.FloorToInt(position)))
            .ToList();
        SimpleBenchmark.Execute(UnityRaycastWithChunkRollback, mapProvider, mapHistory, chunkMeshes, explodedPositions);
        explosionPositions = new List<Vector3>()
        {
            new(120, 10, 133), new(133, 20, 146), new(147, 19, 190), new(215, 8, 168), new(261, 6, 204),
        };
        explodedPositions = explosionPositions
            .SelectMany(position => sphereArea.GetOverlappedBlockPositions(Vector3Int.FloorToInt(position)))
            .ToList();
        SimpleBenchmark.Execute(UnityRaycastWithChunkRollback, mapProvider, mapHistory, chunkMeshes, explodedPositions);
        explosionPositions = new List<Vector3>()
        {
            new(120, 10, 133), new(133, 20, 146)
        };
        explodedPositions = explosionPositions
            .SelectMany(position => sphereArea.GetOverlappedBlockPositions(Vector3Int.FloorToInt(position)))
            .ToList();
        SimpleBenchmark.Execute(UnityRaycastWithChunkRollback, mapProvider, mapHistory, chunkMeshes, explodedPositions);
        explosionPositions = new List<Vector3>()
        {
            new(120, 10, 133)
        };
        explodedPositions = explosionPositions
            .SelectMany(position => sphereArea.GetOverlappedBlockPositions(Vector3Int.FloorToInt(position)))
            .ToList();
        SimpleBenchmark.Execute(UnityRaycastWithChunkRollback, mapProvider, mapHistory, chunkMeshes, explodedPositions);

        server.Stop();
    }

    private void MyRaycast(IMapProvider mapProvider, MapHistory mapHistory)
    {
        var tick = (int) Math.Floor(NetworkTime.localTime * NetworkServer.tickRate);
        CheckCollisionWithBlock(mapProvider, mapHistory, _ray,
            out _, 250, tick - 1);
    }

    private void UnityRaycast()
    {
        Physics.Raycast(_ray, out _, 250, Constants.buildMask);
    }

    private void UnityRaycastWithChunkRollback(IMapProvider mapProvider, MapHistory mapHistory,
        ChunkMesh[] chunkMeshes, List<Vector3Int> explodedPositions)
    {
        var tick = (int) Math.Floor(NetworkTime.localTime * NetworkServer.tickRate);
        var blocksByChunkIndex = explodedPositions
            .GroupBy(mapProvider.GetChunkNumberByGlobalPosition,
                position =>
                    new BlockDataWithPosition(mapProvider.GetLocalPositionByGlobal(position),
                        mapHistory.GetBlockByGlobalPosition(position, tick)))
            .ToDictionary(group => group.Key, group => group.ToList());

        foreach (var (chunkIndex, blocks) in blocksByChunkIndex)
        {
            chunkMeshes[chunkIndex].SpawnBlocks(blocks);
        }

        UnityRaycast();

        blocksByChunkIndex = explodedPositions
            .GroupBy(mapProvider.GetChunkNumberByGlobalPosition,
                position =>
                    new BlockDataWithPosition(mapProvider.GetLocalPositionByGlobal(position), new BlockData()))
            .ToDictionary(group => group.Key, group => group.ToList());

        foreach (var (chunkIndex, blocks) in blocksByChunkIndex)
        {
            chunkMeshes[chunkIndex].SpawnBlocks(blocks);
        }
        // Revert world
    }


    private bool CheckCollisionWithBlock(IMapProvider mapProvider, MapHistory mapHistory, Ray ray,
        out BlockDataWithPosition data, float distance, int tick)
    {
        var startPoint = ray.origin;
        var endPoint = ray.GetPoint(distance);
        foreach (var blockPosition in DDA.Calculate(startPoint, endPoint))
        {
            if (!mapProvider.IsInsideMap(blockPosition.x, blockPosition.y, blockPosition.z))
            {
                break;
            }

            var blockFromPast = mapHistory.GetBlockByGlobalPosition(blockPosition, tick);
            if (blockFromPast.IsSolid())
            {
                data = new BlockDataWithPosition(blockPosition, blockFromPast);
                return true;
            }
        }

        data = new BlockDataWithPosition(Vector3Int.FloorToInt(ray.origin + Constants.worldOffset),
            new BlockData());
        return false;
    }

    private void RegisterDependencies()
    {
        var assetProvider = new AssetProvider();
        AllServices.Container.RegisterSingle<IAssetProvider>(assetProvider);
        AllServices.Container.RegisterSingle<IStaticDataService>(new StaticDataService(assetProvider));
        AllServices.Container.RegisterSingle<IMeshFactory>(new MeshFactory(assetProvider));
        AllServices.Container.RegisterSingle<IGameFactory>(new GameFactory(AllServices.Container));
    }
}