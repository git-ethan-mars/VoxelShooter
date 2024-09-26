using Common;
using Common.StaticData;
using Entities;
using GamePlay.PlayerLogic.Inventory.Tnt;
using Mirror;
using Networking.Messages.Requests;
using Networking.Messages.Responses;
using UnityEngine;

namespace Networking.Host
{
	public partial class MirrorHost : IRequestHandler<TntSpawnRequest>
	{
		public void OnRequestReceived(NetworkConnectionToClient connection, TntSpawnRequest request)
		{
			var playerFound = TryGetPlayerData(connection, out var playerData);
			if (!playerFound || !playerData.IsAlive || playerData.SelectedItemData is not TntData tntData)
			{
				return;
			}

			if (tntData.Amount <= 0)
			{
				return;
			}

			tntData.Amount -= 1;
			connection.Send(new ItemUseResponse(playerData.SelectedSlotIndex, tntData.Amount));
			var raycastResult = Physics.Raycast(request.Ray, out var raycastHit,
				playerData.Characteristic.placeDistance, Constants.buildMask);
			if (!raycastResult)
			{
				return;
			}

			
			var tnt = _entityFactory.CreateTnt(Vector3.zero, Quaternion.identity, tntData);
			_coroutineRunner.StartCoroutine(Utils.DoActionAfterDelay(() => Explode(tnt), tntData.DelayInSeconds));
		}

		private void Explode(Tnt tnt)
		{
			/*if (tnt != null)
			{
				tnt.Explode();
			}*/
		}
	}
}