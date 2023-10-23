using System;
using System.Collections;
using Data;
using Infrastructure;
using Infrastructure.Factory;
using Mirror;
using Networking.Messages.Responses;
using UnityEngine;

namespace Networking.ServerServices
{
    public class BoxDropService
    {
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly int _timeInSeconds;
        private ServerTime _timeLeft;
        private readonly Action _onStop;
        private IEntityFactory _entityFactory;
        private const int SpawnRate = 5;

        public BoxDropService(ICoroutineRunner coroutineRunner, int timeInMinutes, Action onStop,
            IEntityFactory entityFactory)
        {
            _coroutineRunner = coroutineRunner;
            _timeInSeconds = timeInMinutes * 60;
            _onStop = onStop;
            _entityFactory = entityFactory;
        }

        public void Start()
        {
            _coroutineRunner.StartCoroutine(SpawnLootBox());
        }

        private IEnumerator SpawnLootBox()
        {
            _timeLeft = new ServerTime(_timeInSeconds);
            for (var i = 0; i < _timeInSeconds; i++)
            {
                if (i % SpawnRate == 0)
                {
                    _entityFactory.CreateLootBox(new Vector3(100, 100, 100));
                }
                _timeLeft = _timeLeft.Subtract(new ServerTime(1));
                yield return new WaitForSeconds(1);
            }
            _onStop?.Invoke();
        }
    }
}