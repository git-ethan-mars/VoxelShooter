using System.Collections.Generic;
using System.Linq;
using CameraLogic;
using Data;
using Infrastructure.Factory;
using MapLogic;
using PlayerLogic;
using TMPro;
using UI;
using UnityEngine;

namespace Blueprints
{
    public class ConstructionView
    {
        private readonly GameObject _blockInfo;
        private readonly TextMeshProUGUI _blockCountText;
        private readonly Sprite _blockSprite;
        private readonly GameObject _palette;
        private readonly List<GameObject> _transparentConstruction = new();
        private readonly RayCaster _rayCaster;
        private readonly float _placeDistance;
        public Blueprint _blueprint;
        public BlockDataWithPosition[] _rotatedConstruction;
        private readonly MapProvider _mapProvider;
        private readonly Player _player;

        public ConstructionView(IMeshFactory meshFactory, RayCaster rayCaster, Player player, Blueprint blueprint,
            MapProvider mapProvider)
        {
            _rayCaster = rayCaster;
            _placeDistance = player.PlaceDistance;
            _blueprint = blueprint;
            _player = player;
            _rotatedConstruction = _blueprint.blockDataWithPositions;
            _mapProvider = mapProvider;
            for (var i = 0; i < _blueprint.blockDataWithPositions.Length; i++)
            {
                _transparentConstruction.Add(meshFactory.CreateTransparentBlock());
            }
            ChangeTransparentBlockColor();
        }

        public void DisplayTransparentConstruction()
        {
            var raycastResult = _rayCaster.GetRayCastHit(out var raycastHit, _placeDistance, Constants.buildMask);
            if (raycastResult)
            {
                var target = Vector3Int.FloorToInt(raycastHit.point + raycastHit.normal / 2);
                for (var i = 0; i < _rotatedConstruction.Length; i++)
                {
                    _transparentConstruction[i].transform.position = target + Constants.worldOffset + _rotatedConstruction[i].Position;   
                }
            }

            foreach (var block in _transparentConstruction)
            {
                block.SetActive(raycastResult);
            }
            
            ChangeTransparentBlockColor();
        }

        private bool IsPlaceValid()
        {
            foreach (var block in _transparentConstruction)
            {
                var blockPosition = Vector3Int.RoundToInt(block.transform.position - Constants.worldOffset);
                if (_mapProvider.GetBlockByGlobalPosition(blockPosition.x, blockPosition.y, blockPosition.z).IsSolid())
                {
                    return false;
                }
            }
            return true;
        }

        private void ChangeTransparentBlockColor()
        {
            var color = IsPlaceValid() && _player.BlockCount.Value >= _transparentConstruction.Count ? Color.green : Color.red;
            for (var i = 0; i < _rotatedConstruction.Length; i++)
            {
                var floatColor = color;
                var material = _transparentConstruction[i].GetComponentInChildren<MeshRenderer>().material;
                floatColor = new Color(floatColor.r, floatColor.g, floatColor.b, material.color.a);
                material.color = floatColor;
            }
        }
        
        public void Enable()
        {
            foreach (var block in _transparentConstruction)
            {
                block.SetActive(true);
            }
        }

        public void Disable() 
        {
            foreach (var block in _transparentConstruction)
            {
                block.SetActive(false);
            }
        }

        public void Dispose()
        {
            foreach (var block in _transparentConstruction)
            {
                Object.Destroy(block);   
            }
        }
    }
}