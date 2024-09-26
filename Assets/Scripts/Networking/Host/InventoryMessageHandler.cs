using Mirror;
using Networking.Messages.Requests;
using Networking.Messages.Responses;
using PlayerLogic;

namespace Networking.Host
{
	public partial class MirrorHost : IRequestHandler<ChangeSlotRequest>, IRequestHandler<IncrementSlotIndexRequest>,
		IRequestHandler<DecrementSlotIndexRequest>
	{
		public void OnRequestReceived(NetworkConnectionToClient connection, IncrementSlotIndexRequest request)
		{
			/*var result = TryGetPlayerData(connection, out var playerData);
			if (!result || !playerData.IsAlive)
			{
				return;
			}

			playerData.SelectedSlotIndex = (playerData.SelectedSlotIndex + 1 + playerData.ItemData.Count) %
			                               playerData.ItemData.Count;
			connection.Send(new ChangeSlotResponse(playerData.SelectedSlotIndex));
			connection.identity.GetComponent<PlayerBase>().SetItem(playerData.ItemData[playerData.SelectedSlotIndex].ID);
			if (playerData.HasContinuousSound)
			{
				StopContinuousSound(connection.identity);
				playerData.HasContinuousSound = false;
			}*/
		}

		public void OnRequestReceived(NetworkConnectionToClient connection, DecrementSlotIndexRequest request)
		{
			/*
			var result = TryGetPlayerData(connection, out var playerData);
			if (!result || !playerData.IsAlive)
			{
				return;
			}

			playerData.SelectedSlotIndex = (playerData.SelectedSlotIndex - 1 + playerData.Items.Count) %
			                               playerData.Items.Count;
			connection.Send(new ChangeSlotResponse(playerData.SelectedSlotIndex));
			connection.identity.GetComponent<PlayerBase>().SetItem(playerData.Items[playerData.SelectedSlotIndex].id);
			if (playerData.HasContinuousSound)
			{
				StopContinuousSound(connection.identity);
				playerData.HasContinuousSound = false;
			}
		*/
		}

		public void OnRequestReceived(NetworkConnectionToClient connection, ChangeSlotRequest request)
		{
			/*
			var result = TryGetPlayerData(connection, out var playerData);
			if (!result || !playerData.IsAlive)
			{
				return;
			}

			if (request.Index < 0 || request.Index >= playerData.Items.Count)
			{
				return;
			}

			playerData.SelectedSlotIndex = request.Index;
			connection.Send(new ChangeSlotResponse(playerData.SelectedSlotIndex));
			connection.identity.GetComponent<PlayerBase>().SetItem(playerData.Items[playerData.SelectedSlotIndex].id);
			if (playerData.HasContinuousSound)
			{
				StopContinuousSound(connection.identity);
				playerData.HasContinuousSound = false;
			}
		}
	*/
		}
	}
}