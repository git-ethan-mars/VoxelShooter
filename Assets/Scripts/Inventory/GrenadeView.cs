using System;
using Data;
using Mirror;
using Networking.Messages;
using Networking.Messages.Requests;
using Rendering;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory
{
    public class GrenadeView : IInventoryItemView, IConsumable, ILeftMouseButtonDownHandler, ILeftMouseButtonUpHandler
    {
        public Sprite Icon { get; }
        public int Count { get; set; }
        private readonly Raycaster _raycaster;
        private readonly float _maxThrowDuration;
        private readonly float _throwForceModifier;
        private readonly float _minThrowForce;
        private readonly int _itemId;
        private readonly GameObject _grenadeInfo;
        private readonly TextMeshProUGUI _grenadeCountText;
        private readonly Sprite _grenadeCountIcon;
        private readonly Sprite _itemTypeIcon;
        private readonly Image _itemType;
        private float _holdDownStartTime;

        public GrenadeView(Raycaster raycaster, GrenadeItem configuration, Hud hud)
        {
            Icon = configuration.inventoryIcon;
            _maxThrowDuration = configuration.maxThrowDuration;
            _throwForceModifier = configuration.throwForceModifier;
            _itemId = configuration.id;
            _grenadeInfo = hud.itemInfo;
            _grenadeCountText = hud.itemCount;
            _grenadeCountIcon = configuration.countIcon;
            _itemType = hud.itemIcon;
            Count = configuration.count;
            _raycaster = raycaster;
            _minThrowForce = configuration.minThrowForce;
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

        public void OnLeftMouseButtonDown()
        {
            _holdDownStartTime = Time.time;
        }

        public void Unselect()
        {
            _grenadeInfo.SetActive(false);
        }

        public void OnLeftMouseButtonUp()
        {
            var holdTime = Math.Min(Time.time - _holdDownStartTime, _maxThrowDuration);
            var throwForce = Math.Max(holdTime * _throwForceModifier, _minThrowForce);
            NetworkClient.Send(new GrenadeSpawnRequest(_itemId, _raycaster.CentredRay, throwForce));
        }
    }
}