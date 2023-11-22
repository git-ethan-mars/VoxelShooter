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

        public GrenadeView(GrenadeItem configure, Hud hud)
        {
            Icon = configure.inventoryIcon;
            _grenadeInfo = hud.itemInfo;
            _grenadeCountText = hud.itemCount;
            _grenadeCountIcon = configure.countIcon;
            _itemType = hud.itemIcon;
            _count = configure.count;
        }

        public void Enable()
        {
            _grenadeInfo.SetActive(true);
            _itemType.sprite = _grenadeCountIcon;
            _grenadeCountText.SetText(_count.ToString());
        }

        public void OnCountChanged(int count)
        {
            _count = count;
            _grenadeCountText.SetText(_count.ToString());
        }

        public void Disable()
        {
            _grenadeInfo.SetActive(false);
        }
    }
}