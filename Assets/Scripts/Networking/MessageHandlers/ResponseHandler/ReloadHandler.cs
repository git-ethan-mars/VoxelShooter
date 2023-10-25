using System.Collections.Generic;
using Inventory;
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class ReloadResultHandler : ResponseHandler<ReloadResultResponse>
    {
        private readonly List<Slot> _slots;

        public ReloadResultHandler(List<Slot> slots)
        {
            _slots = slots;
        }

        protected override void OnResponseReceived(ReloadResultResponse response)
        {
            var reloading = (IReloading) _slots.Find(slot => slot.InventoryItem.id == response.WeaponId).ItemHandler;
            reloading.TotalBullets = response.TotalBullets;
            reloading.BulletsInMagazine = response.BulletsInMagazine;
            reloading.OnReloadResult();
        }
    }
}