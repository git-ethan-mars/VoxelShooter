using System;
using Data;
using MapLogic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Optimization
{
    public struct ChunkSerializer : IJob
    {
        private NativeList<byte> _serializedChunk;
        private NativeArray<BlockData> _blocks;
        private readonly Color32 _solidColor;
        private int _coloredStart;
        private int _coloredEnd;
        private int _solidStart;
        private int _solidEnd;

        public ChunkSerializer(NativeList<byte> serializedChunk, NativeArray<BlockData> blocks, Color32 solidColor)
        {
            _serializedChunk = serializedChunk;
            _blocks = blocks;
            _solidColor = solidColor;
            _coloredStart = -1;
            _coloredEnd = -1;
            _solidStart = -1;
            _solidEnd = -1;
        }

        public void Execute()
        {
            for (var i = 0; i < _blocks.Length; i++)
            {
                if (_blocks[i].Color.IsEquals(_solidColor))
                {
                    if (IsColoredRunStarted) WriteColoredRun();
                    if (!IsSolidRunStarted) _solidStart = i;
                    _solidEnd = i;
                    continue;
                }

                if (!_blocks[i].Color.IsEquals(BlockColor.empty))
                {
                    if (IsSolidRunStarted) WriteSolidRun();
                    if (!IsColoredRunStarted) _coloredStart = i;
                    _coloredEnd = i;
                }
                else
                {
                    if (IsSolidRunStarted) WriteSolidRun();
                    if (IsColoredRunStarted) WriteColoredRun();
                }
            }

            if (IsSolidRunStarted) WriteSolidRun();
            if (IsColoredRunStarted) WriteColoredRun();
            _serializedChunk.Add((byte) MapRun.End);
        }


        private void WriteSolidRun()
        {
            _serializedChunk.Add((byte) MapRun.Solid);
            AddInt(_solidStart);
            AddInt(_solidEnd);
            _solidStart = -1;
            _solidEnd = -1;
        }

        private void WriteColoredRun()
        {
            _serializedChunk.Add((byte) MapRun.Colored);
            AddInt(_coloredStart);
            AddInt(_coloredEnd);
            for (var i = _coloredStart; i <= _coloredEnd; i++)
            {
                var block = _blocks[i];
                _serializedChunk.Add(block.Color.b);
                _serializedChunk.Add(block.Color.g);
                _serializedChunk.Add(block.Color.r);
                _serializedChunk.Add(block.Color.a);
            }

            _coloredStart = -1;
            _coloredEnd = -1;
        }

        private void AddInt(int value)
        {
            var bytes = BitConverter.GetBytes(value);
            for (var i = 0; i < bytes.Length; i++)
            {
                _serializedChunk.Add(bytes[i]);
            }
        }

        private bool IsSolidRunStarted => _solidStart != -1;
        private bool IsColoredRunStarted => _coloredStart != -1;
    }
}