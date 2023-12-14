using CameraLogic;
using Data;
using Infrastructure;
using Mirror;
using Networking.Messages.Requests;
using PlayerLogic;
using UnityEngine;

namespace Inventory.Block
{
    public class BlockModel : IInventoryItemModel, IConsumable
    {
        public ObservableVariable<int> Count { get; set; }
        public ObservableVariable<Color32> BlockColor { get; }

        private readonly GameObject _transparentBlock;
        private readonly RayCaster _rayCaster;
        private readonly float _placeDistance;

        public BlockModel(BlockItem configure, RayCaster rayCaster, Player player, Color32 initialColor)
        {
            _rayCaster = rayCaster;
            _placeDistance = player.PlaceDistance;
            Count = new ObservableVariable<int>(configure.count);
            BlockColor = new ObservableVariable<Color32>(initialColor);
        }

        public void PlaceBlock()
        {
            var raycastResult = _rayCaster.GetRayCastHit(out var raycastHit, _placeDistance, Constants.buildMask);
            if (raycastResult)
            {
                NetworkClient.Send(new AddBlocksRequest(new BlockDataWithPosition[]
                {
                    new(Vector3Int.FloorToInt(raycastHit.point + raycastHit.normal / 2),
                        new BlockData(BlockColor.Value))
                }));
            }
        }

        public void ChangeColor(Color32 color)
        {
            BlockColor.Value = color;
        }
    }
}