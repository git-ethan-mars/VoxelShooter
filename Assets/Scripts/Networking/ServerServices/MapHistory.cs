using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Infrastructure;
using Mirror;
using UnityEngine;

namespace MapLogic
{
    public class MapHistory
    {
        private const int HistorySize = 30; // 500 

        private readonly ICoroutineRunner _coroutineRunner;
        private readonly IMapProvider _mapProvider;

        private readonly LinkedList<HistoryNode> _history;
        private readonly Dictionary<Vector3Int, BlockData> _currentTickChanges;
        private int _serverTick = int.MinValue;
        private IEnumerator _coroutine;

        public MapHistory(ICoroutineRunner coroutineRunner, IMapProvider mapProvider)
        {
            _coroutineRunner = coroutineRunner;
            _mapProvider = mapProvider;
            _history = new LinkedList<HistoryNode>();
            _currentTickChanges = new Dictionary<Vector3Int, BlockData>();
        }

        public void Start()
        {
            _coroutine = HandleTick();
            _coroutineRunner.StartCoroutine(_coroutine);
        }

        public BlockData GetBlockByGlobalPosition(Vector3Int position, int tick)
        {
            AssertTick(tick);
            var block = _mapProvider.GetBlockByGlobalPosition(position);
            if (tick >= _serverTick)
            {
                return block;
            }

            var currentNode = _history.Last;
            if (currentNode!.Value.ChangeByPosition.TryGetValue(position, out var oldBlock))
            {
                block = oldBlock;
            }

            while (tick != currentNode.Value.Tick)
            {
                currentNode = currentNode.Previous;
                if (currentNode!.Value.ChangeByPosition.TryGetValue(position, out oldBlock))
                {
                    block = oldBlock;
                }
            }

            return block;
        }

        public void UpdateHistory(BlockDataWithPosition block)
        {
            if (_currentTickChanges.ContainsKey(block.Position))
            {
                return;
            }

            _currentTickChanges[block.Position] = _mapProvider.GetBlockByGlobalPosition(block.Position);
        }

        public void Stop()
        {
            _coroutineRunner.StopCoroutine(_coroutine);
        }

        private IEnumerator HandleTick()
        {
            while (true)
            {
                var tick = (int) Math.Floor(NetworkTime.localTime * NetworkServer.tickRate);
                _serverTick = tick;
                if (_history.Count == HistorySize)
                {
                    _history.RemoveFirst();
                }

                _history.AddLast(new HistoryNode(tick - 1,
                    new Dictionary<Vector3Int, BlockData>(_currentTickChanges)));
                _currentTickChanges.Clear();
                yield return new WaitForSeconds(NetworkServer.sendInterval);
            }
        }

        private void AssertTick(int tick)
        {
            if (_serverTick - tick > HistorySize)
            {
                throw new ArgumentException(
                    $"Tick={tick} is out of range. History size = {HistorySize}, server tick = {_serverTick}");
            }
        }


        private class HistoryNode
        {
            public readonly int Tick;
            public readonly Dictionary<Vector3Int, BlockData> ChangeByPosition;

            public HistoryNode(int tick, Dictionary<Vector3Int, BlockData> changeByPosition)
            {
                Tick = tick;
                ChangeByPosition = changeByPosition;
            }
        }
    }
}