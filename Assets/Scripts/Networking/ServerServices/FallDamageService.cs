using System.Collections;
using System.Collections.Generic;
using Data;
using Infrastructure;
using Mirror;
using PlayerLogic;
using UnityEngine;

namespace Networking.ServerServices
{
    public class FallDamageService
    {
        private readonly IServer _server;
        private IEnumerator _coroutine;
        private readonly ICoroutineRunner _networkManager;
        private readonly Dictionary<NetworkConnectionToClient, float> _speedByConnection = new();
        private readonly FallDamageData _fallDamageData;

        public FallDamageService(IServer server, CustomNetworkManager networkManager)
        {
            _server = server;
            _networkManager = networkManager;
            _fallDamageData = networkManager.StaticData.GetFallDamageConfiguration();
        }

        public void Start()
        {
            _coroutine = CheckFallDamage();
            _networkManager.StartCoroutine(_coroutine);
        }

        public void Stop()
        {
            _networkManager.StopCoroutine(_coroutine);
        }

        private IEnumerator CheckFallDamage()
        {
            while (true)
            {
                foreach (var connection in _server.ClientConnections)
                {
                    if (!connection.identity)
                    {
                        continue;
                    }

                    if (!_speedByConnection.ContainsKey(connection))
                    {
                        _speedByConnection[connection] = 0.0f;
                    }
                    else
                    {
                        var result = connection.identity.TryGetComponent<Player>(out var player);
                        if (!result)
                        {
                            continue;
                        }

                        if (_speedByConnection[connection] < -_fallDamageData.minSpeedToDamage
                            && Mathf.Abs(player.Rigidbody.velocity.y) < Constants.Epsilon)
                        {
                            _server.Damage(connection, connection,
                                (int) (-(_speedByConnection[connection] + _fallDamageData.minSpeedToDamage) *
                                       _fallDamageData.damagePerMetersPerSecond));
                        }

                        _speedByConnection[connection] = player.Rigidbody.velocity.y;
                    }
                }

                yield return new WaitForFixedUpdate();
            }
        }
    }
}