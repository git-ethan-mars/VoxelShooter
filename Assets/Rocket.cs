using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Mirror;
using Networking;
using Networking.Messages;
using Networking.Synchronization;
using UnityEngine;

public class Rocket : NetworkBehaviour
{
    private ServerData _serverData;
    private int _radius;
    private int _damage;
    private NetworkConnectionToClient _owner;

    public void Construct(ServerData serverData, RocketLauncherItem rocketData, NetworkConnectionToClient owner)
    {
        _serverData = serverData;
        _radius = rocketData.radius;
        _damage = rocketData.damage;
        _owner = owner;
    }
    
    [Server]
    private void OnCollisionEnter(Collision collision)
    {
        var validPositions = new List<Vector3Int>();
        var grenadePosition = new Vector3Int((int)transform.position.x,
            (int)transform.position.y, (int)transform.position.z);
        for (var x = -_radius; x <= _radius; x++)
        {
            for (var y = -_radius; y <= _radius; y++)
            {
                for (var z = -_radius; z <= _radius; z++)
                {
                    var blockPosition = grenadePosition + new Vector3Int(x, y, z);
                    if (_serverData.Map.IsValidPosition(blockPosition) &&
                        Vector3Int.Distance(blockPosition, grenadePosition) <= _radius)
                        validPositions.Add(blockPosition);
                }
            }
        }

        foreach (var position in validPositions)
        {
            _serverData.Map.SetBlockByGlobalPosition(position, new BlockData());
        }

        NetworkServer.SendToAll(new UpdateMapMessage(validPositions.ToArray(),
            new BlockData[validPositions.Count]));
        NetworkServer.Destroy(gameObject);

        Collider[] hitColliders = Physics.OverlapSphere(grenadePosition, _radius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                var playerPosition = hitCollider.transform.position;
                var distance = Math.Sqrt(
                    (grenadePosition.x - playerPosition.x) * (grenadePosition.x - playerPosition.x) +
                    (grenadePosition.y - playerPosition.y) * (grenadePosition.y - playerPosition.y) +
                    (grenadePosition.z - playerPosition.z) * (grenadePosition.z - playerPosition.z));
                var currentDamage = (int)(_damage - _damage * (distance / _radius));
                var receiver = hitCollider.gameObject.GetComponentInParent<NetworkIdentity>().connectionToClient;
                receiver.identity.GetComponent<HealthSynchronization>().Damage(_owner, receiver, currentDamage);
            }
        }
    }
}
