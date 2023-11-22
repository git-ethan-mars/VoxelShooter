using Data;
using Infrastructure.Factory;
using PlayerLogic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory
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
        private int _count;

        public BlockView(IMeshFactory meshFactory, BlockItem configure, RayCaster rayCaster, Player player,
            Hud hud)
        {
            _rayCaster = rayCaster;
            _placeDistance = player.PlaceDistance;
            _palette = hud.palette;
            _blockInfo = hud.itemInfo;
            _blockImage = hud.itemIcon;
            _blockCountText = hud.itemCount;
            Icon = configure.inventoryIcon;
            _blockSprite = configure.itemSprite;
            _palette.GetComponent<PaletteCreator>().OnColorUpdate += ChangeTransparentBlockColor;
            _count = configure.count;
            _transparentBlock = meshFactory.CreateTransparentGameObject(configure.prefab, Color.black);
            _transparentBlock.SetActive(false);
        }

        public void Enable()
        {
            _transparentBlock.SetActive(true);
            _palette.SetActive(true);
            _blockInfo.SetActive(true);
            _blockImage.sprite = _blockSprite;
            _blockCountText.SetText(_count.ToString());
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

        public void OnCountChanged(int count)
        {
            _count = count;
            _blockCountText.SetText(_count.ToString());
        }


        public void ChangeTransparentBlockColor(Color32 color)
        {
            _transparentBlock.GetComponent<MeshRenderer>().material.color = new Color(color.r / 255f,
                color.g / 255f, color.b / 255f, 100 / 255f);
        }
    }
}