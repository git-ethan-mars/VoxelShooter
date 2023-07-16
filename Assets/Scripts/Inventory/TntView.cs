using System;
using Data;
using Mirror;
using Networking.Messages;
using PlayerLogic;
using Rendering;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory
{
    public class TntView : IInventoryItemView, IConsumable, ILeftMouseButtonDownHandler, IUpdated
    {
        public Sprite Icon { get; }
        public int Count { get; set; }
        private readonly Raycaster _raycaster;
        private readonly GameObject _transparentTnt;
        private readonly int _itemId;
        private readonly GameObject _tntInfo;
        private readonly TextMeshProUGUI _tntCountText;
        private readonly Sprite _tntCountIcon;
        private readonly Sprite _itemTypeIcon;
        private readonly Image _itemType;
        private readonly float _placeDistance;

        public TntView(Raycaster raycaster, TntItem configuration, Hud hud, TransparentMeshPool transparentMeshPool,
            Player player)
        {
            Icon = configuration.inventoryIcon;
            _raycaster = raycaster;
            _placeDistance = player.placeDistance;
            _itemId = configuration.id;
            _tntInfo = hud.itemInfo;
            _tntCountText = hud.itemCount;
            _tntCountIcon = configuration.countIcon;
            _itemType = hud.itemIcon;
            Count = configuration.count;
            _transparentTnt =
                transparentMeshPool.CreateTransparentGameObject(configuration.prefab, new Color32(255, 0, 0, 100));
            _transparentTnt.SetActive(false);
        }


        public void OnCountChanged()
        {
            _tntCountText.SetText(Count.ToString());
        }

        public void Select()
        {
            _transparentTnt.SetActive(true);
            _tntInfo.SetActive(true);
            _itemType.sprite = _tntCountIcon;
            _tntCountText.SetText(Count.ToString());
        }


        public void Unselect()
        {
            _transparentTnt.gameObject.SetActive(false);
            _tntInfo.SetActive(false);
        }

        public void OnLeftMouseButtonDown()
        {
            var raycastResult = _raycaster.GetRayCastHit(out var raycastHit, _placeDistance, Constants.BuildMask);
            if (!raycastResult) return;
            NetworkClient.Send(new TntSpawnRequest(_itemId,
                Vector3Int.FloorToInt(raycastHit.point + raycastHit.normal / 2) +
                GetTntOffsetPosition(raycastHit.normal),
                GetTntRotation(raycastHit.normal),
                Vector3Int.FloorToInt(raycastHit.point + raycastHit.normal / 2)));
        }


        public void InnerUpdate()
        {
            var raycastResult = _raycaster.GetRayCastHit(out var raycastHit, _placeDistance, Constants.BuildMask);
            if (raycastResult)
            {
                _transparentTnt.SetActive(true);
                _transparentTnt.transform.position = Vector3Int.FloorToInt(raycastHit.point + raycastHit.normal / 2) +
                                                     GetTntOffsetPosition(raycastHit.normal);
                _transparentTnt.transform.rotation = GetTntRotation(raycastHit.normal);
            }
            else
            {
                _transparentTnt.SetActive(false);
            }
        }

        private static Vector3 GetTntOffsetPosition(Vector3 normal)
        {
            if (normal == Vector3.up)
            {
                return new Vector3(0.45f, 0, 0.43f);
            }

            if (normal == Vector3.down)
            {
                return new Vector3(0.45f, 1, 0.57f);
            }

            if (normal == Vector3.right)
            {
                return new Vector3(0, 0.57f, 0.57f);
            }

            if (normal == Vector3.left)
            {
                return new Vector3(1, 0.57f, 0.43f);
            }

            if (normal == Vector3.forward)
            {
                return new Vector3(0.43f, 0.57f, 0);
            }

            if (normal == Vector3.back)
            {
                return new Vector3(0.57f, 0.57f, 1);
            }

            throw new ArgumentException("Can't attach tnt to wrong face of block");
        }

        private static Quaternion GetTntRotation(Vector3 normal)
        {
            if (normal == Vector3.up)
            {
                return Quaternion.Euler(0, 0, 0);
            }

            if (normal == Vector3.down)
            {
                return Quaternion.Euler(180, 0, 0);
            }

            if (normal == Vector3.right)
            {
                return Quaternion.Euler(90, 0, -90);
            }

            if (normal == Vector3.left)
            {
                return Quaternion.Euler(90, 0, 90);
            }

            if (normal == Vector3.forward)
            {
                return Quaternion.Euler(90, 0, 0);
            }

            if (normal == Vector3.back)
            {
                return Quaternion.Euler(90, 0, 180);
            }

            throw new ArgumentException("Can't attach tnt to wrong face of block");
        }
    }
}