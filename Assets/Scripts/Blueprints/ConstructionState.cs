using System.Linq;
using CameraLogic;
using Data;
using Infrastructure.Factory;
using Inventory;
using MapLogic;
using Mirror;
using Networking.Messages.Requests;
using PlayerLogic;
using UI;
using UnityEngine;

namespace Blueprints
{
    public class ConstructionState
    {
        private readonly InventoryInput _inventoryInput;
        private readonly RayCaster _rayCaster;
        private readonly float _placeDistance;
        public readonly Blueprint Blueprint;
        private BlockDataWithPosition[] _rotatedConstruction;
        private int _rotationStep;
        private readonly ConstructionView _constructionView;
        private readonly Player _player;
        private readonly MapProvider _mapProvider;

        public ConstructionState(IMeshFactory meshFactory, InventoryInput inventoryInput, RayCaster rayCaster,
            Player player, Hud hud, MapProvider mapProvider, Blueprint blueprint)
        {
            _inventoryInput = inventoryInput;
            Blueprint = blueprint;
            _rotatedConstruction = RotateConstruction();
            _mapProvider = mapProvider;
            _rayCaster = rayCaster;
            _player = player;
            _placeDistance = player.PlaceDistance;
            _constructionView = new ConstructionView(meshFactory, rayCaster, player, blueprint, mapProvider);
        }

        public void Enter()
        {
            _inventoryInput.FirstActionButtonDown += PlaceConstruction;
            _inventoryInput.SecondActionButtonDown += IncrementRotateStep;
            _constructionView.Enable();
        }

        public void Update()
        {
            _constructionView.DisplayTransparentConstruction();
        }

        public void Exit()
        {
            _inventoryInput.FirstActionButtonDown -= PlaceConstruction;
            _inventoryInput.SecondActionButtonDown -= IncrementRotateStep;
            _constructionView.Disable();
        }
        
        private void IncrementRotateStep()
        {
            _rotationStep = (_rotationStep + 1) % 4;
            _rotatedConstruction = RotateConstruction();
            _constructionView._rotatedConstruction = _rotatedConstruction;
        }

        public void Dispose()
        {
            _constructionView.Dispose();
        }
        
        private bool IsPlaceValid(BlockDataWithPosition[] construction)
        {
            for (var i = 0; i < construction.Length; i++)
            {
                if (construction[i].Position.x < 0 || construction[i].Position.y < 0 || construction[i].Position.z < 0 
                    || _mapProvider.GetBlockByGlobalPosition(construction[i].Position.x, construction[i].Position.y, 
                        construction[i].Position.z).IsSolid())
                {
                    return false;
                }
            }
            return true;
        }

        private void PlaceConstruction()
        {
            var raycastResult = _rayCaster.GetRayCastHit(out var raycastHit, _placeDistance, Constants.buildMask);
            var construction = CreateConstruction(raycastHit);

            if (!IsPlaceValid(construction))
            {
                return;
            }
            
            if (raycastResult && _player.BlockCount.Value >= construction.Length)
            {
                NetworkClient.Send(new AddBlocksRequest(construction));
            }
        }

        private BlockDataWithPosition[] CreateConstruction(RaycastHit raycastHit)
        {
            var target = Vector3Int.FloorToInt(raycastHit.point + raycastHit.normal / 2);
            var construction = new BlockDataWithPosition[_rotatedConstruction.Length];
            for (var i = 0; i < construction.Length; i++)
            {
                construction[i] = new BlockDataWithPosition(_rotatedConstruction[i].Position + target,
                    Blueprint.blockDataWithPositions[i].BlockData);
            }

            return construction;
        }

        private BlockDataWithPosition[] RotateConstruction()
        {
            var construction = new BlockDataWithPosition[Blueprint.blockDataWithPositions.Length];
            for (var i = 0; i < construction.Length; i++)
            {
                var position = Blueprint.blockDataWithPositions[i].Position;
                construction[i] = new BlockDataWithPosition(Vector3Int.RoundToInt(Quaternion.Euler(0, 90 * _rotationStep, 0) * position), Blueprint.blockDataWithPositions[i].BlockData);
            }

            return construction;
        }
    }
}