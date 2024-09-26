using System;
using System.Collections.Generic;
using Common.StaticData;
using Entities;
using Entities.PlayerLogic;
using Mirror;
using Networking.Messages.Requests;
using Networking.Messages.Responses;

namespace Networking.Host
{
	public partial class MirrorHost : IRequestHandler<AddBlocksRequest>
	{
		public void OnRequestReceived(NetworkConnectionToClient connection, AddBlocksRequest request)
		{
			var result = TryGetPlayerData(connection, out var playerData);
			if (!result || !playerData.IsAlive || playerData.SelectedItemData is not BlockItemData)
			{
				return;
			}

			var blockItemData = (BlockItemData) playerData.SelectedItemData;
			var validBlocks = new List<BlockDataWithPosition>();
			var blocksUsed = Math.Min(blockItemData.Amount, request.Blocks.Length);
			for (var i = 0; i < blocksUsed; i++)
			{
				var blockPosition = request.Blocks[i].Position;
				foreach (var player in Entity.GetEntitiesByType<Character>())
				{
					var playerPosition = player.transform.position;
					if (playerPosition.x > blockPosition.x
					    && playerPosition.x < blockPosition.x + 1
					    && playerPosition.z > blockPosition.z
					    && playerPosition.z < blockPosition.z + 1
					    && playerPosition.y > blockPosition.y - 2
					    && playerPosition.y < blockPosition.y + 2)
						return;
				}

				if (!MapUpdater.IsDestructiblePosition(blockPosition))
				{
					return;
				}

				var currentBlock = MapUpdater.GetBlockByGlobalPosition(blockPosition);
				if (currentBlock.Equals(request.Blocks[i].BlockData))
				{
					return;
				}

				validBlocks.Add(request.Blocks[i]);
			}

			blockItemData.Amount -= blocksUsed;
			connection.Send(new ItemUseResponse(playerData.SelectedSlotIndex,
				blockItemData.Amount));
			MapUpdater.SetBlocksByGlobalPositions(validBlocks);
		}
	}
}