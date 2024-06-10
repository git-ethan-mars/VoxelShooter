using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CameraLogic;
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
using Networking;
using Networking.ServerServices;
using Optimization;
using Steamworks;
using Tests.EditMode;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;

public class LagCompensationTests
{
    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.

    [UnityTest]
    public IEnumerator LagCompensationSample()
    {
        var raycaster = new RayCaster(SceneView.GetAllSceneCameras().First());
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
        var rangeWeaponValidator = new MockShootValidator(server, mapProvider);
        var explosionBehaviour = new ExplosionBehaviour(server, connection, 8, 250);
        var explosionPositions = new List<Vector3>()
        {
            new(120, 10, 133), new(133, 20, 146), new(147, 19, 190), new(215, 8, 168), new(261, 6, 204),
            new(227, 11, 275),
            new(277, 9, 294), new(288, 8, 219), new(296, 16, 301), new(304, 23, 303), new(349, 20, 334),
            new(339, 26, 344), new(332, 15, 384), new(310, 13, 378), new(299, 31, 364), new(228, 51, 231),
            new(164, 16, 171), new(170, 5, 251), new(206, 5, 256), new(320, 5, 253), new(372, 9, 184)
        };
        for (var i = 0; i < explosionPositions.Count; i++)
        {
            explosionBehaviour.Explode(explosionPositions[i]);
        }

        yield return new WaitForSeconds(NetworkServer.sendInterval);
        var tick = (int) Math.Floor(NetworkTime.localTime * NetworkServer.tickRate);
        var ray = new Ray(new Vector3(256, 128, 256), Vector3.down);
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        for (var i = 0; i < 10000; i++)
        {
            CheckCollisionWithBlock(mapProvider, mapHistory, ray,
                out var blockData, 200, tick - 1);
        }

        stopWatch.Stop();
        Debug.Log(stopWatch.ElapsedMilliseconds);
        
        stopWatch.Restart();
        for (var i = 0; i < 10000; i++)
        {
            Physics.Raycast(ray, out var raycastHit, 200, Constants.buildMask);
        }

        stopWatch.Stop();
        Debug.Log(stopWatch.ElapsedMilliseconds);

        server.Stop();
    }

    private void ShootUnitySolution(RayCaster raycaster, IRangeWeaponValidator rangeWeaponValidator,
        NetworkConnectionToClient connection)
    {
        var result = Physics.Raycast(raycaster.CentredRay, out var hitInfo, 500,
            Constants.attackMask);
        if (result && hitInfo.collider.CompareTag("Chunk"))
        {
            rangeWeaponValidator.Shoot(connection,
                raycaster.CentredRay, false, 0);
        }
    }

    /*private void ShootMySolution(RayCaster raycaster, IRangeWeaponValidator rangeWeaponValidator,
        NetworkConnectionToClient connection)
    {
        var result = CheckCollisionWithBlock(mapProvider);
        if (result && hitInfo.collider.CompareTag("Chunk"))
        {
            rangeWeaponValidator.Shoot(connection,
                raycaster.CentredRay, false, 0);
        }
    }*/

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