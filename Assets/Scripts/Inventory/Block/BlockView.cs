using CameraLogic;
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
        private readonly GameObject _transparentBlock;
        private readonly RayCaster _rayCaster;
        private readonly float _placeDistance;

        public BlockView(IMeshFactory meshFactory, BlockItem configure, RayCaster rayCaster, Player player,
            Hud hud, Color32 initialColor)
        {
            _rayCaster = rayCaster;
            _placeDistance = player.PlaceDistance;
            _palette = hud.Palette.gameObject;
            _blockInfo = hud.ItemInfo;
            _blockImage = hud.ItemIcon;
            _blockCountText = hud.ItemCount;
            Icon = configure.inventoryIcon;
            _blockSprite = configure.itemSprite;
            _transparentBlock = meshFactory.CreateTransparentGameObject(configure.prefab, initialColor);
            _transparentBlock.SetActive(false);
        }

        public void Enable()
        {
            _transparentBlock.SetActive(true);
            _palette.SetActive(true);
            _blockInfo.SetActive(true);
            _blockImage.sprite = _blockSprite;
        }

        public void Disable()
        {
            _palette.SetActive(false);
            _blockInfo.SetActive(false);
            _transparentBlock.SetActive(false);
        }

        public void DisplayTransparentBlock()
        {
            var raycastResult = _rayCaster.GetRayCastHit(out var raycastHit, _placeDistance, Constants.buildMask);
            if (raycastResult)
            {
                _transparentBlock.transform.position = Vector3Int.FloorToInt(raycastHit.point + raycastHit.normal / 2) +
                                                       new Vector3(0.5f, 0.5f, 0.5f);
            }

            _transparentBlock.SetActive(raycastResult);
        }

        public void ChangeTransparentBlockColor(Color32 color)
        {
            var floatColor = (Color) color;
            var material = _transparentBlock.GetComponent<MeshRenderer>().material;
            floatColor = new Color(floatColor.r, floatColor.g, floatColor.b, material.color.a);
            material.color = floatColor;
        }

        public void Dispose()
        {
            Object.Destroy(_transparentBlock);
        }

        public void UpdateAmmoText(string ammoText)
        {
            _blockCountText.SetText(ammoText);
        }
    }
}