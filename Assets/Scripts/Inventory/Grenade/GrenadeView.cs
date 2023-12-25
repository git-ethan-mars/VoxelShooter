using Data;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory.Grenade
{
    public class GrenadeView : IInventoryItemView
    {
        public Sprite Icon { get; }
        private readonly GameObject _grenadeInfo;
        private readonly TextMeshProUGUI _grenadeCountText;
        private readonly Sprite _grenadeCountIcon;
        private readonly Sprite _itemTypeIcon;
        private readonly Image _itemType;
        private int _count;
        private bool _isSelected;

        public GrenadeView(GrenadeItem configure, Hud hud)
        {
            Icon = configure.inventoryIcon;
            _grenadeInfo = hud.ItemInfo;
            _grenadeCountText = hud.ItemCount;
            _grenadeCountIcon = configure.countIcon;
            _itemType = hud.ItemIcon;
            _count = configure.count;
        }

        public void Enable()
        {
            _isSelected = true;
            _grenadeInfo.SetActive(true);
            _itemType.sprite = _grenadeCountIcon;
            _grenadeCountText.SetText(_count.ToString());
        }

        public void OnCountChanged(int count)
        {
            _count = count;
            if (_isSelected)
            {
                _grenadeCountText.SetText(_count.ToString());
            }
        }

        public void Disable()
        {
            _isSelected = false;
            _grenadeInfo.SetActive(false);
        }
    }
}