using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Infrastructure.Factory;
using Mirror;
using Networking;
using Networking.Messages;
using Networking.Synchronization;
using UnityEngine;

public class Rocket : NetworkBehaviour
{
    private ServerData _serverData;
    private IParticleFactory _particleFactory;
    private int _radius;
    private int _damage;
    private NetworkConnectionToClient _owner;

    public void Construct(ServerData serverData, RocketLauncherItem rocketData,
        NetworkConnectionToClient owner, IParticleFactory particleFactory)
    {
        _serverData = serverData;
        _particleFactory = particleFactory;
        _radius = rocketData.radius;
        _damage = rocketData.damage;
        _owner = owner;
    }

    [Server]
    private void OnCollisionEnter(Collision collision)
    {
        var blockPositions = new List<Vector3Int>();
        var blockColors = new List<Color32>();
        var emptyBlock = new BlockData().Color;
        var rocketPosition = new Vector3Int((int) transform.position.x,
            (int) transform.position.y, (int) transform.position.z);
        for (var x = -_radius; x <= _radius; x++)
        {
            for (var y = -_radius; y <= _radius; y++)
            {
                for (var z = -_radius; z <= _radius; z++)
                {
                    var blockPosition = rocketPosition + new Vector3Int(x, y, z);
                    if (!_serverData.Map.IsValidPosition(blockPosition)) continue;
                    var blockData = _serverData.Map.GetBlockByGlobalPosition(blockPosition);
                    if (Vector3Int.Distance(blockPosition, rocketPosition) <= _radius &&
                        !blockData.Color.Equals(emptyBlock))
                    {
                        blockPositions.Add(blockPosition);
                        blockColors.Add(blockData.Color);
                    }
                }
            }
        }

        foreach (var tuple in blockPositions.Zip(blockColors, Tuple.Create))
        {
            _serverData.Map.SetBlockByGlobalPosition(tuple.Item1, new BlockData());
            _particleFactory.CreateRchParticle(tuple.Item1, tuple.Item2);
        }

        NetworkServer.SendToAll(new UpdateMapMessage(blockPositions.ToArray(),
            new BlockData[blockPositions.Count]));
        NetworkServer.Destroy(gameObject);

        Collider[] hitColliders = Physics.OverlapSphere(rocketPosition, _radius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                var playerPosition = hitCollider.transform.position;
                var distance = Math.Sqrt(
                    (rocketPosition.x - playerPosition.x) * (rocketPosition.x - playerPosition.x) +
                    (rocketPosition.y - playerPosition.y) * (rocketPosition.y - playerPosition.y) +
                    (rocketPosition.z - playerPosition.z) * (rocketPosition.z - playerPosition.z));
                var currentDamage = (int) (_damage - _damage * (distance / _radius));
                var receiver = hitCollider.gameObject.GetComponentInParent<NetworkIdentity>().connectionToClient;
                receiver.identity.GetComponent<HealthSynchronization>().Damage(_owner, receiver, currentDamage);
            }
        }
    }
}