using Common.Input;
using Common.StaticData;
using Common.Storage;
using GamePlay.Entities;
using Infrastructure.Factory;
using PlayerLogic;
using PlayerLogic.Rotation;
using TMPro;
using UnityEngine;

namespace Entities.PlayerLogic
{
	public class Character : Entity
	{
		[SerializeField]
		private TextMeshProUGUI nickNameText;

		[SerializeField]
		private Transform itemPosition;
		
		public string NickName;

		public PlayerCharacteristic Characteristic;

		private PlayerCharacteristic _characteristic;
		public float PlaceDistance { get; private set; }
		public PlayerAudio Audio { get; private set; }
		
		public HealthSystem HealthSystem { get; private set; }

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


		public Rigidbody Rigidbody => rigidbody;

		[SerializeField]
		private new Rigidbody rigidbody;

		[SerializeField]
		private AudioSource continuousAudio;

		[SerializeField]
		private AudioSource stepAudio;

		[SerializeField]
		private AudioData stepAudioData;
		
		private IInputService _inputService;
		private IStorageService _storageService;
		
		private IRotation _rotation;
		private Camera _mainCamera;

		private PlayerMovement _movement;
		private float _speed;
		private float _jumpHeight;
		public Inventory.Inventory Inventory => _inventory; 
		private Inventory.Inventory _inventory;

		public void Construct(IInputService inputService, IStorageService storageService, IStaticDataService staticData)
		{
			_inventory = new Inventory.Inventory();
			HealthSystem = new HealthSystem();
			_inputService = inputService;
			_storageService = storageService;
			_movement = new PlayerMovement(hitBox, rigidbody, bodyOrientation);
			Audio = new PlayerAudio(_storageService, stepAudio, stepAudioData, continuousAudio);
		}

		private void Start()
		{
			TurnOffNickName();
			TurnOffBodyRender();
			MountCamera();
			_mainCamera = Camera.main;
			_rotation = new PlayerRotation(_storageService, ZoomService, bodyOrientation, headPivot);
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
			_rotation.Dispose();
			_mainCamera.transform.SetParent(null);
			base.OnDestroy();
			Audio.Dispose();
		}
	}
}