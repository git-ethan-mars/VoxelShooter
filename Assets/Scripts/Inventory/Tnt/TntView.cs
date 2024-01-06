using CameraLogic;
using Data;
using Infrastructure.Factory;
using PlayerLogic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory.Tnt
{
    public class TntView : IInventoryItemView
    {
        public Sprite Icon { get; }
        private readonly RayCaster _rayCaster;
        private readonly GameObject _transparentTnt;
        private readonly GameObject _tntInfo;
        private readonly TextMeshProUGUI _tntCountText;
        private readonly Sprite _tntCountIcon;
        private readonly Sprite _itemTypeIcon;
        private readonly Image _itemType;
        private readonly float _placeDistance;

        public TntView(IMeshFactory meshFactory, RayCaster rayCaster, TntItem configuration, Player player,
            Hud hud)
        {
            Icon = configuration.inventoryIcon;
            _rayCaster = rayCaster;
            _placeDistance = player.PlaceDistance;
            _tntInfo = hud.ItemInfo;
            _tntCountText = hud.ItemCount;
            _tntCountIcon = configuration.countIcon;
            _itemType = hud.ItemIcon;
            _transparentTnt = meshFactory.CreateTransparentGameObject(configuration.prefab, Color.red);
            _transparentTnt.SetActive(false);
        }

        public void Enable()
        {
            _transparentTnt.SetActive(true);
            _tntInfo.SetActive(true);
            _itemType.sprite = _tntCountIcon;
        }

        public void Update()
        {
            var raycastResult = _rayCaster.GetRayCastHit(out var raycastHit, _placeDistance, Constants.buildMask);
            if (raycastResult)
            {
                _transparentTnt.SetActive(true);
                _transparentTnt.transform.position = Vector3Int.FloorToInt(raycastHit.point + raycastHit.normal / 2) +
                                                     TntPlaceHelper.GetTntOffsetPosition(raycastHit.normal);
                _transparentTnt.transform.rotation = TntPlaceHelper.GetTntRotation(raycastHit.normal);
            }
            else
            {
                _transparentTnt.SetActive(false);
            }
        }

        public void UpdateAmmoText(string ammoText)
        {
            _tntCountText.SetText(ammoText);
        }

        public void Disable()
        {
            _transparentTnt.gameObject.SetActive(false);
            _tntInfo.SetActive(false);
        }

        public void Dispose()
        {
            Object.Destroy(_transparentTnt);
        }
    }
}