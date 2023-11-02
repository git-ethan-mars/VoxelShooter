using System;
using System.Collections;
using Data;
using Infrastructure;
using Mirror;
using Networking.Messages.Responses;
using UnityEngine;

namespace Networking.ServerServices
{
    public class ServerTimer
    {
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly int _timeInSeconds;
        private ServerTime _timeLeft;
        private readonly Action _onStop;

        public ServerTimer(ICoroutineRunner coroutineRunner, int timeInMinutes, Action onStop)
        {
            _coroutineRunner = coroutineRunner;
            _timeInSeconds = timeInMinutes * 60;
            _onStop = onStop;
        }

        public void Start()
        {
            _coroutineRunner.StartCoroutine(SendTime());
        }

        private IEnumerator SendTime()
        {
            _timeLeft = new ServerTime(_timeInSeconds);
            for (var i = 0; i < _timeInSeconds; i++)
            {
                NetworkServer.SendToReady(new GameTimeResponse(_timeLeft));
                _timeLeft = _timeLeft.Subtract(new ServerTime(1));
                yield return new WaitForSeconds(1);
            }

            _onStop?.Invoke();
        }
    }
}