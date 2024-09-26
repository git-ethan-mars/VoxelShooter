using Networking.Messages.Responses;

namespace Networking.Client
{
	public partial class MirrorClient : IResponseHandler<ShootResultResponse>, IResponseHandler<ReloadResultResponse>
	{
		public void OnResponseReceived(ShootResultResponse response)
		{
			//((RangeWeaponModel) NetworkClient.localPlayer.GetComponent<PlayerBase>().Inventory.GetModel(response.SlotIndex)).BulletsInMagazine = response.BulletsInMagazine;
		}

		public void OnResponseReceived(ReloadResultResponse response)
		{
			/*var rangeWeapon = (RangeWeaponModel) NetworkClient.localPlayer.GetComponent<PlayerBase>().Inventory.GetModel(response.SlotIndex);
			rangeWeapon.TotalBullets = response.TotalBullets;
			*/
		//	rangeWeapon.BulletsInMagazine = response.BulletsInMagazine;
		}
	}
}