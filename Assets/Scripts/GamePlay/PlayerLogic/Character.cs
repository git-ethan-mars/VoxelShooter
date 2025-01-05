using GamePlay.Data;
using GamePlay.Entities;
using GamePlay.Services;
using TMPro;
using UnityEngine;
using VoxelMap;

namespace GamePlay
{
    public class Character : Entity
    {
        [SerializeField] private TextMeshProUGUI nickNameText;

        [SerializeField] private Transform itemPosition;

        [SerializeField] private MeshRenderer[] bodyParts;

        [SerializeField] private GameObject nickNameCanvas;

        [SerializeField] private Transform bodyOrientation;

        [SerializeField] private Transform headPivot;

        [SerializeField] private Transform cameraMountPoint;

        [SerializeField] private CapsuleCollider hitBox;

        [SerializeField] private new Rigidbody rigidbody;

        [SerializeField] private AudioSource continuousAudio;

        [SerializeField] private AudioSource stepAudio;

        [SerializeField] private AudioData stepAudioData;

        public HealthSystem HealthSystem { get; private set; }
        public InventorySystem Inventory { get; private set; }
        public Rigidbody RigidBody => rigidbody;

        private IInputService _inputService;
        private IStorageService _storageService;
        private IStaticDataService _staticData;
        private MapProvider _mapProvider;

        private Camera _mainCamera;
        private IRotation _rotation;
        private PlayerMovement _movement;
        private float _speed;
        private float _jumpHeight;


        public void Construct(IInputService inputService, IStorageService storageService, IStaticDataService staticData)
        {
            _inputService = inputService;
            _storageService = storageService;
            _staticData = staticData;
            _mainCamera = Camera.main;
        }

        public void Initialize(InventorySystem inventory)
        {
            var playerData = GetComponent<IPlayerData>();
            _movement = new PlayerMovement(hitBox, rigidbody, bodyOrientation);
            var characteristic = _staticData.GetPlayerCharacteristic(playerData.GameClass);
            _speed = characteristic.speed;
            _jumpHeight = characteristic.jumpHeight;
            Inventory = inventory;
            foreach (InventoryItem item in Inventory.Items)
            {
                item.transform.SetParent(transform, false);
            }
            
            HealthSystem = new HealthSystem(characteristic.maxHealth);
            HealthSystem.Increase(characteristic.maxHealth);
            nickNameText.SetText(playerData.NickName);
            TurnOffNickName();
            TurnOffBodyRender();
            MountCamera();
            _rotation = new PlayerRotation(_storageService, bodyOrientation, headPivot);

        }

        private void Update()
        {
            _movement.Move(_inputService.Axis, _speed);

            if (_inputService.IsJumpButtonDown())
            {
                _movement.Jump(_jumpHeight);
            }

            _rotation.Rotate(_inputService.MouseAxis);
        }

        private void FixedUpdate()
        {
            _movement.FixedUpdate();
        }

        public void Heal(int heal)
        {
            HealthSystem.Increase(heal);
        }

        public void Damage(int damage)
        {
            HealthSystem.Decrease(damage);
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

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _rotation.Dispose();
            _mainCamera.transform.SetParent(null);
        }
    }
}