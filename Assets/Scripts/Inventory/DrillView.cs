using Data;
using Mirror;
using Networking.Messages.Requests;
using PlayerLogic;
using Rendering;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory
{
    public class DrillView : IInventoryItemView, IConsumable, ILeftMouseButtonDownHandler, IUpdated
    {
        public Sprite Icon { get; }
        public int Count { get; set; }
        private readonly Raycaster _raycaster;
        private readonly float _delayInSeconds;
        private readonly int _itemId;
        private readonly GameObject _drillInfo;
        private readonly TextMeshProUGUI _drillCountText;
        private readonly Sprite _drillCountIcon;
        private readonly Sprite _itemTypeIcon;
        private readonly Image _itemType;
        private CubeRenderer _cubeRenderer;
        private readonly int _range;

        public DrillView(Raycaster raycaster, DrillItem configuration, Hud hud, Player player)
        {
            Icon = configuration.inventoryIcon;
            _itemId = configuration.id;
            _drillInfo = hud.itemInfo;
            _drillCountText = hud.itemCount;
            _drillCountIcon = configuration.countIcon;
            _itemType = hud.itemIcon;
            Count = configuration.count;
            _raycaster = raycaster;
            _range = configuration.range;
            _cubeRenderer = new CubeRenderer(player.GetComponent<LineRenderer>(), raycaster, _range);
        }

        public void OnCountChanged()
        {
            _drillCountText.SetText(Count.ToString());
        }

        public void Select()
        {
            _drillInfo.SetActive(true);
            _itemType.sprite = _drillCountIcon;
            _drillCountText.SetText(Count.ToString());
            _cubeRenderer.EnableCube();
        }
        
        public void Unselect()
        {
            _drillInfo.SetActive(false);
            _cubeRenderer.DisableCube();
        }
        
        public void InnerUpdate()
        {
            _cubeRenderer.UpdateCube(false);
        }

        public void OnLeftMouseButtonDown()
        {
            var raycastResult = _raycaster.GetRayCastHit(out var raycastHit, _range, Constants.BuildMask);
            if (!raycastResult) return;
            NetworkClient.Send(new DrillSpawnRequest(_itemId,
                _raycaster.CentredRay));
        }
    }
}