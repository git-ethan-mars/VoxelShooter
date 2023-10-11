using System.Collections.Generic;
using Inventory;
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class ReloadHandler : ResponseHandler<ReloadResponse>
    {
        private readonly List<Slot> _slots;

        public ReloadHandler(List<Slot> slots)
        {
            _slots = slots;
        }

        protected override void OnResponseReceived(ReloadResponse response)
        {
            var reloading = (IReloading) _slots.Find(slot => slot.InventoryItem.id == response.WeaponId).ItemHandler;
            reloading.TotalBullets = response.TotalBullets;
            reloading.BulletsInMagazine = response.BulletsInMagazine;
            reloading.OnReloadResult();
        }
    }
}