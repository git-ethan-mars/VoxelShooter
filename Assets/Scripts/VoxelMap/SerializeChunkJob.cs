using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
namespace VoxelMap
{
	[BurstCompile]
	public struct SerializeChunkJob : IJob
	{
		private NativeList<byte> _buffer;
		private NativeArray<VoxelData> _chunkVoxels;
		private readonly Color32 _innerColor;
		private int _coloredStart;
		private int _coloredEnd;
		private int _solidStart;
		private int _solidEnd;

		public SerializeChunkJob(NativeList<byte> buffer, NativeArray<VoxelData> chunkVoxels, Color32 innerColor)
		{
			_buffer = buffer;
			_chunkVoxels = chunkVoxels;
			_innerColor = innerColor;
			_coloredStart = -1;
			_coloredEnd = -1;
			_solidStart = -1;
			_solidEnd = -1;
		}

		public void Execute()
		{
			for (var i = 0; i < _chunkVoxels.Length; i++)
			{
				if (_chunkVoxels[i].Color.IsEqual(_innerColor))
				{
					if (IsColoredRunStarted)
					{
						WriteColoredRun();
					}
					if (!IsSolidRunStarted)
					{
						_solidStart = i;
					}
					_solidEnd = i;
				}

				else if (_chunkVoxels[i].IsSolid())
				{
					if (IsSolidRunStarted)
					{
						WriteSolidRun();
					}

					if (!IsColoredRunStarted)
					{
						_coloredStart = i;
					}
					
					_coloredEnd = i;
				}
				else
				{
					if (IsSolidRunStarted)
					{
						WriteSolidRun();
					}

					if (IsColoredRunStarted)
					{
						WriteColoredRun();
					}
				}
			}

			if (IsSolidRunStarted)
			{
				WriteSolidRun();
			}
			if (IsColoredRunStarted)
			{
				WriteColoredRun();
			}
			_buffer.Add((byte)MapRun.ChunkEnd);
		}


		private void WriteSolidRun()
		{
			_buffer.Add((byte)MapRun.Solid);
			_buffer.AddInt(_solidStart);
			_buffer.AddInt(_solidEnd);
			_solidStart = -1;
			_solidEnd = -1;
		}

		private void WriteColoredRun()
		{
			_buffer.Add((byte)MapRun.Colored);
			_buffer.AddInt(_coloredStart);
			_buffer.AddInt(_coloredEnd);
			for (int i = _coloredStart; i <= _coloredEnd; i++)
			{
				VoxelData voxel = _chunkVoxels[i];
				_buffer.Add(voxel.Color.b);
				_buffer.Add(voxel.Color.g);
				_buffer.Add(voxel.Color.r);
				_buffer.Add(voxel.Color.a);
			}

			_coloredStart = -1;
			_coloredEnd = -1;
		}

		private bool IsSolidRunStarted => _solidStart != -1;
		private bool IsColoredRunStarted => _coloredStart != -1;
	}
}