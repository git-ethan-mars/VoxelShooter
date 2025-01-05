using System;
using System.Collections.Generic;
using System.IO;
using GamePlay.MapFeatures;
using Networking.Messages.Responses;
using VoxelMap;

namespace Networking.Client
{
	public partial class MirrorClient : IResponseHandler<MapNameResponse>, IResponseHandler<DownloadMapResponse>, IResponseHandler<UpdateMapResponse>, IResponseHandler<FallBlockResponse>
	{
		private readonly List<Voxel> _blockBuffer = new();
		private byte[] _byteChunks;
		
		private string _mapName;
		private Map _map;
		private MapProvider _mapProvider;
		private readonly FallMeshGenerator _fallMeshGenerator;

		public void OnResponseReceived(MapNameResponse response)
		{
			_mapName = response.MapName;
		}
		
		public void OnResponseReceived(DownloadMapResponse message)
		{
			_byteChunks ??= new byte[message.TotalBytes];
			var lastByte = message.StartByte + _byteChunks.Length;
			Array.Copy(message.ByteChunk, 0, _byteChunks, message.StartByte, _byteChunks.Length);
			
			MapLoadProgressed?.Invoke((float) lastByte / message.TotalBytes);
			if (lastByte < message.TotalBytes)
			{
				return;
			}

			using var memoryStream = new MemoryStream(_byteChunks);
			MapLoaded?.Invoke(_mapName, MapReader.ReadFromStream(memoryStream));
		}

		public void OnResponseReceived(UpdateMapResponse message)
		{
			_blockBuffer.AddRange(message.Voxels);
			if (_mapProvider == null)
			{
				return;
			}

			_mapProvider.SetVoxelsByGlobalPositions(_blockBuffer);	
			_blockBuffer.Clear();
		}

		public void OnResponseReceived(FallBlockResponse response)
		{
			_fallMeshGenerator.GenerateFallVoxels(response.Voxels);
		}
	}
}