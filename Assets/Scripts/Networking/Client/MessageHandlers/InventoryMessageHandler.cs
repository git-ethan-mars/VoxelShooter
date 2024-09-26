using Networking.Messages.Responses;

namespace Networking.Client
{
	public partial class MirrorClient : IResponseHandler<ChangeSlotResponse>, IResponseHandler<ItemUseResponse>
	{
		public void OnResponseReceived(ChangeSlotResponse response)
		{
		//	NetworkClient.localPlayer.GetComponent<PlayerBase>().Inventory.SwitchActiveSlot(response.Index);
		}

		public void OnResponseReceived(ItemUseResponse response)
		{
		//	((IConsumable) NetworkClient.localPlayer.GetComponent<PlayerBase>().Inventory.GetModel(response.SlotIndex)).Amount =
		//		response.Count;
		}
	}
}