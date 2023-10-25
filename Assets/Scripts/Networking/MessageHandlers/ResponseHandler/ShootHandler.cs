using System.Collections.Generic;
using Inventory;
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class ShootResultHandler : ResponseHandler<ShootResultResponse>
    {
        private readonly List<Slot> _slots;

        public ShootResultHandler(List<Slot> slots)
        {
            _slots = slots;
        }

        protected override void OnResponseReceived(ShootResultResponse response)
        {
            var shooting = (IShooting) _slots.Find(slot => slot.InventoryItem.id == response.WeaponId).ItemHandler;
            shooting.BulletsInMagazine = response.BulletsInMagazine;
            shooting.OnShootResult();
        }
    }
}