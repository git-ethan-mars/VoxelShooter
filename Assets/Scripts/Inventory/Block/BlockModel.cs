using Data;
using Infrastructure;
using Mirror;
using Networking.Messages.Requests;
using PlayerLogic;
using UnityEngine;

namespace Inventory
{
    public class BlockModel : IInventoryItemModel, IConsumable
    {
        public ObservableVariable<int> Count { get; set; }
        public ObservableVariable<Color32> BlockColor { get; }

        private readonly GameObject _transparentBlock;
        private readonly RayCaster _rayCaster;
        private readonly float _placeDistance;

        public BlockModel(BlockItem configure, RayCaster rayCaster, Player player)
        {
            _rayCaster = rayCaster;
            _placeDistance = player.PlaceDistance;
            Count = new ObservableVariable<int>(configure.count);
            BlockColor = new ObservableVariable<Color32>(Color.black);
        }

        public void PlaceBlock()
        {
            var raycastResult = _rayCaster.GetRayCastHit(out var raycastHit, _placeDistance, Constants.buildMask);
            if (raycastResult)
            {
                NetworkClient.Send(new AddBlocksRequest(
                    new[] {Vector3Int.FloorToInt(raycastHit.point + raycastHit.normal / 2)},
                    new[] {new BlockData(BlockColor.Value)}));
            }
        }

        public void ChangeColor(Color32 color)
        {
            BlockColor.Value = color;
        }
    }
}