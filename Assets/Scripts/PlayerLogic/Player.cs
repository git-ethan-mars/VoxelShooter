using System.Collections.Generic;
using CameraLogic;
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
using UI.SettingsMenu;
using UnityEngine;

namespace PlayerLogic
{
    public class Player : NetworkBehaviour
    {
        public ObservableVariable<int> Health;
        public float PlaceDistance { get; private set; }

        public PlayerRotation Rotation => _rotation;

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

        [SerializeField]
        private AudioSource continuousAudio;

        [SerializeField]
        private AudioSource stepAudio;

        [SerializeField]
        private AudioData stepAudioData;

        public ZoomService ZoomService { get; private set; }
        public bool IsInitialized { get; private set; }
        public PlayerAudio Audio { get; private set; }

        private Camera _mainCamera;

        private IInputService _inputService;
        private InventorySystem _inventory;
        private Hud _hud;

        private PlayerMovement _movement;
        private float _speed;
        private float _jumpHeight;

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
            _speed = speed;
            _jumpHeight = jumpHeight;
            _mainCamera = Camera.main;
            ZoomService = new ZoomService(_mainCamera);
            _rotation = new PlayerRotation(storageService, ZoomService, bodyOrientation, headPivot);
            _hud = uiFactory.CreateHud(this, inputService);
            _inventory = new InventorySystem(_inputService, staticData, meshFactory, itemIds, _hud,
                this);
            var volumeSettings = storageService.Load<VolumeSettingsData>(Constants.VolumeSettingsKey);
            Audio ??= new PlayerAudio(stepAudio, stepAudioData, continuousAudio);
            Audio.ChangeSoundMultiplier(volumeSettings.SoundVolume);
            TurnOffNickName();
            TurnOffBodyRender();
            MountCamera();
            IsInitialized = true;
        }

        private void Start()
        {
            Audio ??= new PlayerAudio(stepAudio, stepAudioData, continuousAudio);
            _movement = new PlayerMovement(hitBox, rigidbody, bodyOrientation);
        }

        private void Update()
        {
            if (!isLocalPlayer)
            {
                if (_movement.GetHorizontalVelocity().magnitude > Constants.Epsilon && _movement.IsGrounded())
                {
                    Audio.EnableStepSound();
                }
                else
                {
                    Audio.DisableStepSound();
                }

                return;
            }

            _movement.Move(_inputService.Axis, _speed);

            if (_inputService.IsJumpButtonDown())
            {
                _movement.Jump(_jumpHeight);
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

        public void ShowHud()
        {
            if (IsInitialized)
            {
                _hud.CanvasGroup.alpha = 1.0f;
            }
        }

        public void HideHud()
        {
            if (IsInitialized)
            {
                _hud.CanvasGroup.alpha = 0.0f;
            }
        }

        private void TurnOffBodyRender()
        {
            foreach (var bodyPart in bodyParts)
            {
                bodyPart.enabled = false;
            }
        }

        private void TurnOffNickName()
        {
            nickNameCanvas.SetActive(false);
        }

        private void MountCamera()
        {
            var cameraTransform = _mainCamera.transform;
            cameraTransform.SetParent(cameraMountPoint.transform);
            cameraTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        public override void OnStopLocalPlayer()
        {
            _mainCamera.transform.SetParent(null);
            _inventory.OnDestroy();
            Destroy(_hud.gameObject);
        }
    }
}