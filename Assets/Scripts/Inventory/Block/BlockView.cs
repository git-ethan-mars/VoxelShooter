﻿using CameraLogic;
using Data;
using Infrastructure.Factory;
using PlayerLogic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory.Block
{
    public class BlockView : IInventoryItemView
    {
        public Sprite Icon { get; }
        private readonly GameObject _blockInfo;
        private readonly TextMeshProUGUI _blockCountText;
        private readonly Image _blockImage;
        private readonly Sprite _blockSprite;
        private readonly GameObject _palette;
        private readonly MeshRenderer _transparentBlock;
        private readonly RayCaster _rayCaster;
        private readonly float _placeDistance;
        private int _count;
        private bool _isSelected;

        public BlockView(IMeshFactory meshFactory, BlockItem configure, RayCaster rayCaster, Player player,
            Hud hud, Color32 initialColor)
        {
            _rayCaster = rayCaster;
            _placeDistance = player.PlaceDistance;
            _palette = hud.palette.gameObject;
            _blockInfo = hud.itemInfo;
            _blockImage = hud.itemIcon;
            _blockCountText = hud.itemCount;
            Icon = configure.inventoryIcon;
            _blockSprite = configure.itemSprite;
            _count = configure.count;
            _transparentBlock = meshFactory.CreateTransparentGameObject(configure.prefab, initialColor)
                .GetComponent<MeshRenderer>();
            _transparentBlock.gameObject.SetActive(false);
        }

        public void Enable()
        {
            _isSelected = true;
            _transparentBlock.gameObject.SetActive(true);
            _palette.SetActive(true);
            _blockInfo.SetActive(true);
            _blockImage.sprite = _blockSprite;
            _blockCountText.SetText(_count.ToString());
        }

        public void Disable()
        {
            _isSelected = false;
            _palette.SetActive(false);
            _blockInfo.SetActive(false);
            _transparentBlock.gameObject.SetActive(false);
        }

        public void DisplayTransparentBlock()
        {
            var raycastResult = _rayCaster.GetRayCastHit(out var raycastHit, _placeDistance, Constants.buildMask);
            if (raycastResult)
            {
                _transparentBlock.transform.position = Vector3Int.FloorToInt(raycastHit.point + raycastHit.normal / 2) +
                                                       new Vector3(0.5f, 0.5f, 0.5f);
            }

            _transparentBlock.gameObject.SetActive(raycastResult);
        }

        public void OnCountChanged(int count)
        {
            _count = count;
            if (_isSelected)
            {
                _blockCountText.SetText(_count.ToString());
            }
        }

        public void ChangeTransparentBlockColor(Color32 color)
        {
            var floatColor = (Color) color;
            var material = _transparentBlock.material;
            floatColor = new Color(floatColor.r, floatColor.g, floatColor.b, material.color.a);
            material.color = floatColor;
        }
    }
}