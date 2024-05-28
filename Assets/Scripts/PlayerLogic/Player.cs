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
using Networking;
using Networking.Prediction.Player;
using TMPro;
using UI;
using UI.SettingsMenu;
using UnityEngine;

namespace PlayerLogic
{
    public class Player : NetworkBehaviour
    {
        public ObservableVariable<int> Health { get; private set; }
        public float PlaceDistance { get; private set; }
        public PlayerRotation Rotation { get; private set; }
        public ZoomService ZoomService { get; private set; }
        public PlayerAudio Audio { get; private set; }
        public bool IsInitialized { get; private set; }

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

        public Rigidbody Rigidbody => rigidbody;

        [SerializeField]
        private new Rigidbody rigidbody;

        [SerializeField]
        private NetworkedPlayerMovement predictedMovement;

        [SerializeField]
        private AudioSource continuousAudio;

        [SerializeField]
        private AudioSource stepAudio;

        [SerializeField]
        private AudioData stepAudioData;

        private IUIFactory _uiFactory;
        private IMeshFactory _meshFactory;
        private IInputService _inputService;
        private IStorageService _storageService;
        private IStaticDataService _staticData;

        private InventorySystem _inventory;
        private Camera _mainCamera;
        private Hud _hud;

        public void Construct(IInputService inputService, IStorageService storageService, IStaticDataService staticData,
            IUIFactory uiFactory,
            IMeshFactory meshFactory)
        {
            _uiFactory = uiFactory;
            _meshFactory = meshFactory;
            _inputService = inputService;
            _storageService = storageService;
            _staticData = staticData;
            var volumeSettings = storageService.Load<VolumeSettingsData>(Constants.VolumeSettingsKey);
            Audio = new PlayerAudio(stepAudio, stepAudioData, continuousAudio);
            Audio.ChangeSoundMultiplier(volumeSettings.SoundVolume);
            storageService.DataSaved += OnDataSaved;
        }

        public void ConstructLocalPlayer(IClient client, float placeDistance, List<int> itemIds, float speed,
            float jumpHeight,
            int health)
        {
            PlaceDistance = placeDistance;
            Health = new ObservableVariable<int>(health);
            _mainCamera = Camera.main;
            ZoomService = new ZoomService(_mainCamera);
            Rotation = new PlayerRotation(_storageService, ZoomService, bodyOrientation, headPivot);
            _hud = _uiFactory.CreateHud(client, this, _inputService);
            _inventory = new InventorySystem(_inputService, _staticData, _meshFactory, itemIds, _hud, this);
            TurnOffNickName();
            TurnOffBodyRender();
            MountCamera();
            IsInitialized = true;
            predictedMovement.Construct(_inputService, speed, jumpHeight);
        }

        private void Update()
        {
            if (!isLocalPlayer)
            {
                if (predictedMovement.GetHorizontalVelocity().magnitude > Constants.Epsilon &&
                    predictedMovement.IsGrounded())
                {
                    Audio.EnableStepSound();
                }
                else
                {
                    Audio.DisableStepSound();
                }
            }
            else
            {
                Rotation.Rotate(_inputService.MouseAxis);
                _inventory.Update();
            }
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
            foreach (var part in bodyParts)
            {
                part.enabled = false;
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

        public override void OnStopClient()
        {
            _storageService.DataSaved -= OnDataSaved;
        }

        public override void OnStopLocalPlayer()
        {
            _mainCamera.transform.SetParent(null);
            _inventory.OnDestroy();
            Destroy(_hud.gameObject);
        }

        private void OnDataSaved(ISettingsData data)
        {
            if (data is VolumeSettingsData volumeSetting)
            {
                Audio.ChangeSoundMultiplier(volumeSetting.SoundVolume);
            }
        }
    }
}