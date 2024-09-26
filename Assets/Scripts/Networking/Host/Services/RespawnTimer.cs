using System;
using System.Collections;
using Common.StaticData;
using Cysharp.Threading.Tasks;
using Infrastructure;
using Mirror;
using Networking.Messages.Responses;
using UnityEngine;

namespace Networking.Host.Services
{
    public class RespawnTimer
    {
        private readonly int _respawnTime;
        private readonly NetworkConnectionToClient _connection;
        private readonly Action _onStop;
        private ServerTime _timeLeft;

        public RespawnTimer(NetworkConnectionToClient connection, int respawnTime,
            Action onStop)
        {
            _respawnTime = respawnTime;
            _connection = connection;
            _onStop = onStop;
        }

        public void Start()
        {
           SendTime();
        }

        private async UniTaskVoid SendTime()
        {
            _timeLeft = new ServerTime(_respawnTime);
            for (var i = 0; i <= _respawnTime; i++)
            {
                _connection.Send(new RespawnTimeResponse(_timeLeft));
                _timeLeft = _timeLeft.Subtract(new ServerTime(1));
                await UniTask.WaitForSeconds(1);
            }
            
            _onStop?.Invoke();
        }
    }
}