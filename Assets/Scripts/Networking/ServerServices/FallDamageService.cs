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
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly Dictionary<NetworkConnectionToClient, float> _speedByConnection = new();
        private readonly FallDamageData _fallDamageData;

        public FallDamageService(IServer server, ICoroutineRunner coroutineRunner)
        {
            _server = server;
            _coroutineRunner = coroutineRunner;
            _fallDamageData = server.Data.StaticData.GetFallDamageConfiguration();
        }
        
        public void Start()
        {
            _coroutine = CheckFallDamage();
            _coroutineRunner.StartCoroutine(_coroutine);
        }

        public void Stop()
        {
            _coroutineRunner.StopCoroutine(_coroutine);
        }
        
        private IEnumerator CheckFallDamage()
        {
            while (true)
            {
                foreach (var connection in _server.Data.ClientConnections)
                {
                    if (!connection.identity) continue;
                    if (!_speedByConnection.ContainsKey(connection))
                        _speedByConnection[connection] = 0;
                    else
                    {
                        var result = connection.identity.TryGetComponent<Player>(out var player);
                        var velocity = player.Rigidbody.velocity.y;
                        if (!result) continue;
                        if (_speedByConnection[connection] < -_fallDamageData.minSpeedToDamage 
                            && Mathf.Abs(velocity) < Constants.Epsilon)
                        {
                            _server.Damage(connection, connection,
                                (int)(-(_speedByConnection[connection] + _fallDamageData.minSpeedToDamage) *
                                      _fallDamageData.damagePerMetersPerSecond));
                        }
                        _speedByConnection[connection] = velocity;
                    }
                }

                yield return new WaitForFixedUpdate();
            }
        }
    }
}