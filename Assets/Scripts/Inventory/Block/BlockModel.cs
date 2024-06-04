using System;
using System.Linq;
using CameraLogic;
using Data;
using Geometry;
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
            get => _blockItemData.Amount;
            set
            {
                _blockItemData.Amount = value;
                AmountChanged?.Invoke(value);
            }
        }

        public ObservableVariable<Color32> BlockColor { get; }

        private readonly GameObject _transparentBlock;
        private readonly RayCaster _rayCaster;
        private readonly float _placeDistance;
        private readonly BlockItemData _blockItemData;

        private Vector3Int _startLinePosition;
        private bool _isStartPositionSet;

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

        public void StartLine()
        {
            var raycastResult = _rayCaster.GetRayCastHit(out var raycastHit, _placeDistance, Constants.buildMask);
            _isStartPositionSet = raycastResult;
            if (!raycastResult)
            {
                return;
            }

            _startLinePosition = Vector3Int.FloorToInt(raycastHit.point + raycastHit.normal / 2);
        }

        public void EndLine()
        {
            var raycastResult = _rayCaster.GetRayCastHit(out var raycastHit, _placeDistance, Constants.buildMask);
            if (!raycastResult || !_isStartPositionSet)
            {
                return;
            }

            var endLinePosition = Vector3Int.FloorToInt(raycastHit.point + raycastHit.normal / 2);
            var positions = VoxelLineDrawer.Calculate(_startLinePosition, endLinePosition, VoxelLineDrawer.Connectivity.Four)
                .Select(position => new BlockDataWithPosition(Vector3Int.FloorToInt(position), new BlockData(BlockColor.Value))).ToArray();
            NetworkClient.Send(new AddBlocksRequest(positions));
        }

        public void ChangeColor(Color32 color)
        {
            BlockColor.Value = color;
        }
    }
}