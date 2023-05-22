using System;
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
    public class GrenadeView : IInventoryItemView, IConsumable, ILeftMouseButtonDownHandler
    {
        public Sprite Icon { get; }
        public int Count { get; set; }
        private readonly Raycaster _raycaster;
        private readonly float _delayInSeconds;
        private readonly int _radius;
        private readonly int _damage;
        private readonly int _throwForce;
        private readonly int _itemId;
        private readonly GameObject _grenadeInfo;
        private readonly TextMeshProUGUI _grenadeCountText;
        private readonly Sprite _grenadeCountIcon;
        private readonly Sprite _itemTypeIcon;
        private readonly Image _itemType;

        public GrenadeView(Raycaster raycaster, GrenadeItem configuration, Hud hud)
        {
            Icon = configuration.inventoryIcon;
            _delayInSeconds = configuration.delayInSeconds;
            _radius = configuration.radius;
            _damage = configuration.damage;
            _throwForce = configuration.throwForce;
            _itemId = configuration.id;
            _grenadeInfo = hud.itemInfo;
            _grenadeCountText = hud.itemCount;
            _grenadeCountIcon = configuration.countIcon;
            _itemType = hud.itemIcon;
            Count = configuration.count;
            _raycaster = raycaster;
        }
        

        public void OnCountChanged()
        {
            _grenadeCountText.SetText(Count.ToString());
        }

        public void Select()
        {
            _grenadeInfo.SetActive(true);
            _itemType.sprite = _grenadeCountIcon;
            _grenadeCountText.SetText(Count.ToString());
        }


        public void Unselect()
        {
            _grenadeInfo.SetActive(false);
        }

        public void OnLeftMouseButtonDown()
        {
            var raycastResult = _raycaster.GetRayCastHit(out var raycastHit);
            var ray = _raycaster.GetRay();
            NetworkClient.Send(new GrenadeSpawnRequest(_itemId, ray.direction,
                Vector3Int.FloorToInt(raycastHit.point + raycastHit.normal / 2), 
                _delayInSeconds, _radius, _damage, _throwForce));
        }
    }
}