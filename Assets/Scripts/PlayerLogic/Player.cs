using System.Collections.Generic;
using Data;
using Infrastructure;
using Infrastructure.Factory;
using Infrastructure.Services.Input;
using Infrastructure.Services.StaticData;
using Infrastructure.Services.Storage;
using Inventory;
using Mirror;
using TMPro;
using UI;
using UnityEngine;

namespace PlayerLogic
{
    public class Player : NetworkBehaviour
    {
        public ObservableVariable<int> Health;
        public float PlaceDistance { get; private set; }

        [SerializeField]
        private TextMeshProUGUI nickNameText;

        [SerializeField]
        private Transform itemPosition;

        public Transform ItemPosition => itemPosition;

        [SerializeField]
        private MeshRenderer[] bodyParts;

        [SerializeField]
        private GameObject nickNameCanvas;

        public Transform BodyOrientation => bodyOrientation;

        [SerializeField]
        private Transform bodyOrientation;

        [SerializeField]
        private Transform headPivot;

        [SerializeField]
        private Transform cameraMountPoint;

        [SerializeField]
        private CapsuleCollider hitBox;

        [SerializeField]
        private new Rigidbody rigidbody;

        private Hud _hud;

        private IInputService _inputService;
        private InventorySystem _inventory;

        private PlayerMovement _movement;
        private PlayerRotation _rotation;


        public void Construct(IUIFactory uiFactory, IMeshFactory meshFactory, IInputService inputService,
            IStorageService storageService,
            IStaticDataService staticData,
            float placeDistance,
            List<int> itemIds, float speed, float jumpHeight, int health)
        {
            PlaceDistance = placeDistance;
            Health = new ObservableVariable<int>(health);
            _inputService = inputService;
            _movement = new PlayerMovement(hitBox, rigidbody, bodyOrientation, speed, jumpHeight);
            var sensitivity = storageService.Load<MouseSettingsData>(Constants.MouseSettingKey).GeneralSensitivity;
            _rotation = new PlayerRotation(bodyOrientation, headPivot, sensitivity);
            _hud = uiFactory.CreateHud(this, inputService);
            _inventory = new InventorySystem(_inputService, staticData, meshFactory, itemIds, _hud, this);
            TurnOffNickName();
            TurnOffBodyRender();
            MountCamera();
        }

        private void Update()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            _movement.Move(_inputService.Axis);

            if (_inputService.IsJumpButtonDown())
            {
                _movement.Jump();
            }

            _rotation.Rotate(_inputService.MouseAxis);
            _inventory.Update();
        }

        private void FixedUpdate()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            _movement.FixedUpdate();
        }

        public void SetNickName(string nickName)
        {
            nickNameText.SetText(nickName);
        }

        private void TurnOffBodyRender()
        {
            for (var i = 0; i < bodyParts.Length; i++)
            {
                bodyParts[i].GetComponent<MeshRenderer>().enabled = false;
            }
        }

        private void TurnOffNickName()
        {
            nickNameCanvas.SetActive(false);
        }

        private void MountCamera()
        {
            var cameraTransform = Camera.main.gameObject.transform;
            cameraTransform.parent = cameraMountPoint.transform;
            cameraTransform.position = cameraMountPoint.position;
            cameraTransform.rotation = cameraMountPoint.rotation;
            Camera.main.fieldOfView = Constants.DefaultFov;
        }

        private void OnDestroy()
        {
            if (_hud != null)
            {
                _inventory.Clear();
                Destroy(_hud.gameObject);
            }
        }
    }
}