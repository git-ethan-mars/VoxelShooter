using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace VoxelMap
{
    [BurstCompile]
    public struct ChunkSerializer : IJob
    {
        private NativeList<byte> _serializedChunk;
        private NativeArray<VoxelData> _voxels;
        private readonly Color32 _innerColor;
        private int _coloredStart;
        private int _coloredEnd;
        private int _solidStart;
        private int _solidEnd;

        public ChunkSerializer(NativeList<byte> serializedChunk, NativeArray<VoxelData> voxels, Color32 innerColor)
        {
            _serializedChunk = serializedChunk;
            _voxels = voxels;
            _innerColor = innerColor;
            _coloredStart = -1;
            _coloredEnd = -1;
            _solidStart = -1;
            _solidEnd = -1;
        }

        public void Execute()
        {
            for (var i = 0; i < _voxels.Length; i++)
            {
                if (_voxels[i].Color.IsEqual(_innerColor))
                {
                    if (IsColoredRunStarted) WriteColoredRun();
                    if (!IsSolidRunStarted) _solidStart = i;
                    _solidEnd = i;
                    continue;
                }

                if (_voxels[i].IsSolid())
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
                var voxel = _voxels[i];
                _serializedChunk.Add(voxel.Color.b);
                _serializedChunk.Add(voxel.Color.g);
                _serializedChunk.Add(voxel.Color.r);
                _serializedChunk.Add(voxel.Color.a);
            }

            _coloredStart = -1;
            _coloredEnd = -1;
        }

        private void AddInt(int value)
        {
            _serializedChunk.Add((byte) (value & 255));
            _serializedChunk.Add((byte) (value >> 8 & 255));
            _serializedChunk.Add((byte) (value >> 16 & 255));
            _serializedChunk.Add((byte) (value >> 24 & 255));
        }

        private bool IsSolidRunStarted => _solidStart != -1;
        private bool IsColoredRunStarted => _coloredStart != -1;
    }
}