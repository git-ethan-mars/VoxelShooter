using System;
using System.Collections.Generic;
using System.IO;
using Common;
using Networking.Messages.Responses;

namespace Networking.Client
{
	public partial class MirrorClient : IResponseHandler<MapNameResponse>, IResponseHandler<DownloadMapResponse>, IResponseHandler<UpdateMapResponse>, IResponseHandler<FallBlockResponse>
	{
		private readonly List<BlockDataWithPosition> _blockBuffer = new();
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
			
			_map.UpdateBlocks(_blockBuffer);
			_blockBuffer.Clear();
		}

		public void OnResponseReceived(UpdateMapResponse message)
		{
			_blockBuffer.AddRange(message.Blocks);
			if (_map != null)
			{
				_map.UpdateBlocks(_blockBuffer);	
			}

			_blockBuffer.Clear();
		}

		public void OnResponseReceived(FallBlockResponse response)
		{
			_fallMeshGenerator.GenerateFallBlocks(response.Blocks);
		}
	}
}