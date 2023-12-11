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
        private Dictionary<NetworkConnectionToClient, float> _fallSpeedDictionary = new();
        private readonly FallDamageData _fallDamageData;

        public FallDamageService(Server server, ICoroutineRunner coroutineRunner)
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
                    if (!_fallSpeedDictionary.ContainsKey(connection))
                        _fallSpeedDictionary[connection] = 0;
                    else
                    {
                        var result = connection.identity.gameObject.TryGetComponent<Player>(out var player);
                        if (!result) continue;
                        if (IsGrounded(player.Rigidbody, player.HitBox) 
                            && _fallSpeedDictionary[connection] < -Constants.Epsilon
                            && player.Rigidbody.velocity.y > -Constants.Epsilon
                            && player.Rigidbody.velocity.y < Constants.Epsilon)
                        {
                            if (_fallSpeedDictionary[connection] < -_fallDamageData.minSpeedToDamage)
                                _server.Damage(connection, connection, 
                                    (int)(-(_fallSpeedDictionary[connection] + _fallDamageData.minSpeedToDamage) * _fallDamageData.damageMultiplier));
                            _fallSpeedDictionary[connection] = 0;
                        }
                        _fallSpeedDictionary[connection] = player.Rigidbody.velocity.y;
                    }
                }

                yield return new WaitForFixedUpdate();
            }
        }
        
        private bool IsGrounded(Rigidbody rigidbody, CapsuleCollider hitBox)
        {
            var isGrounded = Physics.CheckBox(rigidbody.position + hitBox.height / 2 * Vector3.down,
                new Vector3(hitBox.radius / 2, Constants.Epsilon, hitBox.radius / 2),
                Quaternion.identity, Constants.buildMask);
            return isGrounded;
        }
    }
}