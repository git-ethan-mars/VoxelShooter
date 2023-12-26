using System;
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
        public event Action<int> AmountChanged;

        public int Amount
        {
            get => _blockItemData.Count;
            set
            {
                _blockItemData.Count = value;
                AmountChanged?.Invoke(value);
            }
        }

        public ObservableVariable<Color32> BlockColor { get; }

        private readonly GameObject _transparentBlock;
        private readonly RayCaster _rayCaster;
        private readonly float _placeDistance;
        private readonly BlockItemData _blockItemData;


        public BlockModel(BlockItemData data, RayCaster rayCaster, Player player, Color32 initialColor)
        {
            _rayCaster = rayCaster;
            _placeDistance = player.PlaceDistance;
            _blockItemData = data;
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