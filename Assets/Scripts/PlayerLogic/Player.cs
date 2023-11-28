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

        public AudioSource ContinuousAudio => continuousAudio;

        [SerializeField]
        private AudioSource stepAudio;

        [SerializeField]
        private AudioData stepAudioData;

        private Hud _hud;

        private IInputService _inputService;
        private InventorySystem _inventory;

        private PlayerMovement _movement;
        private PlayerRotation _rotation;
        private PlayerAudio _audio;


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
            _inventory = new InventorySystem(_inputService, staticData, meshFactory, storageService, itemIds, _hud,
                this);
            TurnOffNickName();
            TurnOffBodyRender();
            MountCamera();
        }

        private void Start()
        {
            _audio = new PlayerAudio(stepAudio, stepAudioData);
        }

        private void Update()
        {
            if (Vector3.Scale(rigidbody.velocity, new Vector3(1, 0, 1)).magnitude > Constants.Epsilon)
            {
                _audio.EnableStepSound();
            }
            else
            {
                _audio.DisableStepSound();
            }

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