using System;
using Data;
using Mirror;
using Networking.Messages;
using Rendering;
using UnityEngine;

namespace Inventory
{
    public class TntView : IInventoryItemView, ILeftMouseButtonDownHandler, IUpdated
    {
        public Sprite Icon { get; }
        private readonly Raycaster _raycaster;
        private readonly GameObject _transparentTnt;
        private readonly float _delayInSeconds;
        private readonly int _radius;
        private readonly int _itemId;

        public TntView(Raycaster raycaster, TntItem configuration)
        {
            Icon = configuration.inventoryIcon;
            _delayInSeconds = configuration.delayInSeconds;
            _radius = configuration.radius;
            _itemId = configuration.id;
            _raycaster = raycaster;
            var transparentMeshRenderer = new TransparentMeshRenderer();
            _transparentTnt =
                transparentMeshRenderer.CreateTransparentGameObject(configuration.prefab, new Color32(255, 0, 0, 255));
        }

        public void Select()
        {
            _transparentTnt.SetActive(true);
        }


        public void Unselect()
        {
            _transparentTnt.gameObject.SetActive(false);
        }

        public void OnLeftMouseButtonDown()
        {
            var raycastResult = _raycaster.GetRayCastHit(out var raycastHit);
            if (!raycastResult) return;
            NetworkClient.Send(new TntSpawnRequest(_itemId,Vector3Int.FloorToInt(raycastHit.point + raycastHit.normal / 2) +
                                                   GetTntOffsetPosition(raycastHit.normal),
                GetTntRotation(raycastHit.normal), _delayInSeconds,
                Vector3Int.FloorToInt(raycastHit.point + raycastHit.normal / 2), _radius));
            
        }


        public void InnerUpdate()
        {
            var raycastResult = _raycaster.GetRayCastHit(out var raycastHit);
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