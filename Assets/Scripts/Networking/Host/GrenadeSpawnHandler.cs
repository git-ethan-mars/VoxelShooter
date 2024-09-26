using Common.StaticData;
using Explosions;
using Mirror;
using Networking.Messages.Requests;
using Networking.Messages.Responses;
using UnityEngine;

namespace Networking.Host
{
	public partial class MirrorHost : IRequestHandler<GrenadeSpawnRequest>
	{
		public void OnRequestReceived(NetworkConnectionToClient connection, GrenadeSpawnRequest request)
		{
			var result = TryGetPlayerData(connection, out var playerData);
			if (!result || !playerData.IsAlive || playerData.SelectedItemData is not GrenadeData grenadeItem)
			{
				return;
			}

			var grenadeData = (GrenadeData) playerData.ItemData[playerData.SelectedSlotIndex];
			if (grenadeData.Amount <= 0)
			{
				return;
			}

			grenadeData.Amount -= 1;
			connection.Send(new ItemUseResponse(playerData.SelectedSlotIndex, grenadeData.Amount));
			var explosion = new ExplosionBehaviour(this, connection, grenadeItem.Radius, grenadeItem.Damage);
			var grenade = _entityFactory.CreateGrenade(request.Ray.origin, grenadeItem);
			grenade.GetComponent<Rigidbody>().AddForce(request.Ray.direction * request.ThrowForce);
			_coroutineRunner.StartCoroutine(Utils.DoActionAfterDelay(() => explosion.Explode(grenade.transform.position),
				grenadeItem.DelayInSeconds));
		}
	}
}