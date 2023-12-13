using System.Collections;
using Data;
using Mirror;
using Networking.Messages.Responses;
using UnityEngine;

namespace Networking.ServerServices
{
    public class ServerTimer
    {
        private readonly NetworkManager _networkManager;
        private readonly int _timeInSeconds;
        private ServerTime _timeLeft;
        private IEnumerator coroutine;

        public ServerTimer(NetworkManager networkManager, int timeInMinutes)
        {
            _networkManager = networkManager;
            _timeInSeconds = timeInMinutes * 60;
        }

        public void Start()
        {
            coroutine = SendTime();
            _networkManager.StartCoroutine(coroutine);
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

            _networkManager.StopHost();
        }
    }
}