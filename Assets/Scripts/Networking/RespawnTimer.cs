using System;
using System.Collections;
using Data;
using Infrastructure;
using Mirror;
using Networking.Messages;
using UnityEngine;

namespace Networking
{
    public class RespawnTimer
    {
        private readonly int _respawnTime;
        private readonly NetworkConnectionToClient _connection;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly Action _onStop;
        private ServerTime _timeLeft;

        public RespawnTimer(ICoroutineRunner coroutineRunner, NetworkConnectionToClient connection, int respawnTime,
            Action onStop)
        {
            _coroutineRunner = coroutineRunner;
            _respawnTime = respawnTime;
            _connection = connection;
            _onStop = onStop;
        }

        public void Start()
        {
            _coroutineRunner.StartCoroutine(SendTime());
        }

        private IEnumerator SendTime()
        {
            _timeLeft = new ServerTime(_respawnTime);
            for (var i = 0; i <= _respawnTime; i++)
            {
                _connection.Send(new RespawnTimeMessage(_timeLeft));
                _timeLeft = _timeLeft.Subtract(new ServerTime(1));
                yield return new WaitForSeconds(1);
            }
            _onStop?.Invoke();
        }
    }
}