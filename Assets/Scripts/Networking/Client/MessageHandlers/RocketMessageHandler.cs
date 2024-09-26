using Networking.Messages.Responses;

namespace Networking.Client
{
	public partial class MirrorClient : IResponseHandler<RocketSpawnResponse>, IResponseHandler<RocketReloadResponse>
	{
		public void OnResponseReceived(RocketSpawnResponse response)
		{
			/*
			((RocketLauncherModel) NetworkClient.localPlayer.GetComponent<PlayerBase>().Inventory.GetModel(response.SlotIndex))
				.ChargedRockets = response.ChargedRockets;
		*/
		}

		public void OnResponseReceived(RocketReloadResponse response)
		{
			/*
			var rocketLauncher =
				(RocketLauncherModel) NetworkClient.localPlayer.GetComponent<PlayerBase>().Inventory.GetModel(response.SlotIndex);
			rocketLauncher.CarriedRockets = response.CarriedRockets;
			rocketLauncher.ChargedRockets = response.ChargedRockets;
		*/
		}
	}
}