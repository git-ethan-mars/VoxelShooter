using Data;
using Mirror;
using Networking.Messages;
using Rendering;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory
{
    public class BlockView : IInventoryItemView, IConsumable, ILeftMouseButtonDownHandler, IUpdated
    {
        public int Count { get; set; }
        public Sprite Icon { get; }
        private Color32 _currentColor;
        private readonly GameObject _blockInfo;
        private readonly TextMeshProUGUI _blockCountText;
        private readonly int _id;
        private readonly Image _blockImage;
        private readonly Sprite _blockSprite;
        private readonly GameObject _palette;
        private readonly GameObject _transparentBlock;
        private readonly Raycaster _rayCaster;


        public BlockView(GameObject hud, BlockItem configuration, TransparentMeshPool transparentMeshPool,
            Raycaster raycaster)
        {
            _palette = hud.GetComponent<Hud>().palette;
            _blockInfo = hud.GetComponent<Hud>().itemInfo;
            _blockImage = hud.GetComponent<Hud>().itemIcon;
            _blockCountText = hud.GetComponent<Hud>().itemCount;
            Icon = configuration.inventoryIcon;
            _blockSprite = configuration.itemSprite;
            _palette.GetComponent<PaletteCreator>().OnColorUpdate += UpdateColor;
            Count = configuration.count;
            _id = configuration.id;
            _rayCaster = raycaster;
            _transparentBlock =
                transparentMeshPool.CreateTransparentGameObject(configuration.prefab, _currentColor);
            _transparentBlock.SetActive(false);
        }
        

        public void OnCountChanged()
        {
            _blockCountText.SetText(Count.ToString());
        }

        public void Select()
        {
            _transparentBlock.SetActive(true);
            _palette.SetActive(true);
            _blockInfo.SetActive(true);
            _blockImage.sprite = _blockSprite;
            _blockCountText.SetText(Count.ToString());
        }

        public void Unselect()
        {
            _palette.SetActive(false);
            _blockInfo.SetActive(false);
            _transparentBlock.SetActive(false);
        }

        public void OnLeftMouseButtonDown()
        {
            PlaceBlock(_currentColor);
        }

        public void InnerUpdate()
        {
            var raycastResult = _rayCaster.GetRayCastHit(out var raycastHit);
            if (!raycastResult)
                _transparentBlock.SetActive(false);
            else
            {
                _transparentBlock.SetActive(true);
                _transparentBlock.transform.position = Vector3Int.FloorToInt(raycastHit.point + raycastHit.normal / 2) +
                                                       new Vector3(0.5f, 0.5f, 0.5f);
            }
        }

        private void PlaceBlock(Color32 color)
        {
            var raycastResult = _rayCaster.GetRayCastHit(out var raycastHit);
            if (!raycastResult) return;
            NetworkClient.Send(new AddBlocksRequest(
                new[] {Vector3Int.FloorToInt(raycastHit.point + raycastHit.normal / 2)},
                new[] {new BlockData(color)}, _id));
        }


        private void UpdateColor(Color32 newColor)
        {
            _currentColor = newColor;
            _transparentBlock.GetComponent<MeshRenderer>().material.color = new Color(newColor.r / 255f,
                newColor.g / 255f, newColor.b / 255f, 100 / 255f);
        }
    }
}