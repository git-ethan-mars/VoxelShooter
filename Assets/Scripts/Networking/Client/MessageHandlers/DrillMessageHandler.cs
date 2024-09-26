using Networking.Messages.Responses;

namespace Networking.Client
{
	public partial class MirrorClient : IResponseHandler<DrillSpawnResponse>, IResponseHandler<DrillReloadResponse>
	{
		public void OnResponseReceived(DrillSpawnResponse response)
		{
		//	((DrillModel) NetworkClient.localPlayer.GetComponent<PlayerBase>().Inventory.GetModel(response.SlotIndex)).ChargedDrills = response.ChargedDrills;
		}

		public void OnResponseReceived(DrillReloadResponse response)
		{
			/*
			var drill = (DrillModel) NetworkClient.localPlayer.GetComponent<PlayerBase>().Inventory.GetModel(response.SlotIndex);
			drill.CarriedDrills = response.Amount;
			drill.ChargedDrills = response.ChargedDrills;
		*/
		}
	}
}