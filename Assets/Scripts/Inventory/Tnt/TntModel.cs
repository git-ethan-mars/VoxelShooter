using Data;
using Infrastructure;
using Mirror;
using Networking.Messages.Requests;
using PlayerLogic;
using UnityEngine;

namespace Inventory
{
    public class TntModel : IInventoryItemModel, IConsumable
    {
        public ObservableVariable<int> Count { get; set; }
        private readonly RayCaster _rayCaster;
        private readonly float _placeDistance;

        public TntModel(RayCaster rayCaster, Player player, TntItem configure)
        {
            _rayCaster = rayCaster;
            _placeDistance = player.PlaceDistance;
            Count = new ObservableVariable<int>(configure.count);
        }

        public void PlaceTnt()
        {
            var raycastResult = _rayCaster.GetRayCastHit(out var raycastHit, _placeDistance, Constants.buildMask);
            if (!raycastResult) return;
            NetworkClient.Send(new TntSpawnRequest(Vector3Int.FloorToInt(raycastHit.point + raycastHit.normal / 2) +
                                                   TntPlaceHelper.GetTntOffsetPosition(raycastHit.normal),
                TntPlaceHelper.GetTntRotation(raycastHit.normal),
                Vector3Int.FloorToInt(raycastHit.point + raycastHit.normal / 2)));
        }
    }
}