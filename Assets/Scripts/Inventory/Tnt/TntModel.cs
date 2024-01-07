using System;
using CameraLogic;
using Data;
using Mirror;
using Networking.Messages.Requests;
using PlayerLogic;

namespace Inventory.Tnt
{
    public class TntModel : IInventoryItemModel, IConsumable
    {
        public event Action ModelChanged;

        public int Amount
        {
            get => _tntData.Amount;
            set
            {
                _tntData.Amount = value;
                ModelChanged?.Invoke();
            }
        }

        private readonly RayCaster _rayCaster;
        private readonly float _placeDistance;
        private readonly TntItemData _tntData;

        public TntModel(RayCaster rayCaster, Player player, TntItemData tntData)
        {
            _rayCaster = rayCaster;
            _placeDistance = player.PlaceDistance;
            _tntData = tntData;
        }

        public void PlaceTnt()
        {
            var raycastResult = _rayCaster.GetRayCastHit(out _, _placeDistance, Constants.buildMask);
            if (!raycastResult)
            {
                return;
            }

            NetworkClient.Send(new TntSpawnRequest(_rayCaster.CentredRay));
        }
    }
}