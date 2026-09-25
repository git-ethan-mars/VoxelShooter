using System.Collections.Generic;
using System.Linq;
using Data;
using Mirror;
using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;
using VoxelMap;

namespace GamePlay
{
	public class Spectator : NetworkBehaviour
	{
		private const float SensitivityMultiplier = 50.0f;
		private const float Speed = 10.0f;
		private const float AccelerationMultiplier = 2.5f;
		private const float DistanceToTarget = 5.0f;
		private const float CameraRadius = 0.3f;
		private const float WallMargin = 0.6f;
		private const float PitchLimit = 89.9f;

		[SerializeField] private Rigidbody rigidBody;

		[SyncVar] private uint _killerNetId;

		private IInputService _inputService;
		private CameraProvider _cameraProvider;
		private IStorageService _storageService;
		private EntityContainer _entityContainer;
		private MapProvider _mapProvider;

		private float _sensitivity;
		private float _xRotation;
		private float _yRotation;
		private Entity _target;
		private SpectatingMode _mode;

		private Quaternion ViewRotation => Quaternion.Euler(_xRotation, _yRotation, 0.0f);

		[Inject]
		private void Construct(IInputService inputService, IStorageService storageService, CameraProvider cameraProvider,
			EntityContainer entityContainer, MapProvider mapProvider)
		{
			_inputService = inputService;
			_cameraProvider = cameraProvider;
			_storageService = storageService;
			_entityContainer = entityContainer;
			_mapProvider = mapProvider;
		}

		[Server]
		public void Initialize(NetworkConnectionToClient killer)
		{
			_killerNetId = killer != null && killer.identity != null ? killer.identity.netId : 0;
		}

		public override void OnStartLocalPlayer()
		{
			_cameraProvider.MainCamera.transform.SetParent(transform);

			_sensitivity = _storageService.Load<MouseSettingsData>(IStorageService.MouseSettingsKey).GeneralSensitivity;
			_storageService.Subscribe<MouseSettingsData>(ChangeMouseSettings).AddTo(this);

			Entity initialTarget = GetKillerCharacter() ?? GetRandomTarget();

			if (initialTarget != null)
			{
				Follow(initialTarget);
			}
			else
			{
				EnterFreeView();
			}
		}

		private void Update()
		{
			if (!isLocalPlayer)
			{
				return;
			}

			Rotate(_inputService.MouseAxis);

			if (_inputService.IsFirstActionButtonDown() || (_mode == SpectatingMode.Follow && _target == null))
			{
				FollowNextTarget();
			}

			if (_mode == SpectatingMode.Follow && _inputService.Axis != Vector2.zero)
			{
				EnterFreeView();
			}

			if (_mode == SpectatingMode.FreeView)
			{
				Move(_inputService.Axis);
			}
		}

		// The camera is placed after movement, interpolation and network updates of this frame.
		private void LateUpdate()
		{
			if (!isLocalPlayer)
			{
				return;
			}

			if (_mode == SpectatingMode.Follow && _target != null)
			{
				transform.position = _target.transform.position;
				PlaceFollowCamera();
			}
			else
			{
				_cameraProvider.MainCamera.transform.SetPositionAndRotation(transform.position, ViewRotation);
			}
		}

		public override void OnStopLocalPlayer()
		{
			base.OnStopLocalPlayer();

			if (_cameraProvider.MainCamera.transform.parent == transform)
			{
				_cameraProvider.MainCamera.transform.SetParent(null);
			}
		}

		private void FollowNextTarget()
		{
			Entity nextTarget = GetNextTarget();

			if (nextTarget != null)
			{
				Follow(nextTarget);
			}
			else
			{
				EnterFreeView();
			}
		}

		private void Follow(Entity target)
		{
			_mode = SpectatingMode.Follow;
			_target = target;

			// The spectator is snapped to the target every frame, so physics and interpolation must stay out of it.
			if (!rigidBody.isKinematic)
			{
				rigidBody.linearVelocity = Vector3.zero;
				rigidBody.isKinematic = true;
			}

			rigidBody.interpolation = RigidbodyInterpolation.None;
			transform.position = target.transform.position;
			PlaceFollowCamera();
		}

		private void EnterFreeView()
		{
			Vector3 cameraPosition = _cameraProvider.MainCamera.transform.position;

			_mode = SpectatingMode.FreeView;
			_target = null;

			Vector3 position = ClampInsideWalls(cameraPosition);
			transform.position = position;
			rigidBody.position = position;
			rigidBody.isKinematic = false;
			rigidBody.interpolation = RigidbodyInterpolation.Interpolate;
			rigidBody.linearVelocity = Vector3.zero;
		}

		private void Move(Vector2 direction)
		{
			float speed = _inputService.IsSprintButtonHold() ? Speed * AccelerationMultiplier : Speed;
			Quaternion rotation = ViewRotation;

			// Moved by velocity, so the map walls stop the spectator.
			rigidBody.linearVelocity = (rotation * Vector3.forward * direction.x + rotation * Vector3.right * direction.y) * speed;
		}

		// Orbits the camera around the target and pulls it closer when a map wall is behind it.
		private void PlaceFollowCamera()
		{
			Quaternion rotation = ViewRotation;
			Vector3 back = rotation * Vector3.back;
			float distance = DistanceToTarget;

			if (Physics.SphereCast(transform.position, CameraRadius, back, out RaycastHit hit, DistanceToTarget,
				    LayerMasks.WallMask, QueryTriggerInteraction.Ignore))
			{
				distance = hit.distance;
			}

			_cameraProvider.MainCamera.transform.SetPositionAndRotation(transform.position + back * distance, rotation);
		}

		private Vector3 ClampInsideWalls(Vector3 position)
		{
			Map map = _mapProvider.Map.CurrentValue;

			if (map == null)
			{
				return position;
			}

			return new Vector3(
				Mathf.Clamp(position.x, WallMargin, map.Width - WallMargin),
				Mathf.Clamp(position.y, WallMargin, map.Height - WallMargin),
				Mathf.Clamp(position.z, WallMargin, map.Depth - WallMargin));
		}

		private Character GetKillerCharacter()
		{
			if (_killerNetId == 0)
			{
				return null;
			}

			return _entityContainer.GetEntitiesByType<Character>()
				.FirstOrDefault(character => character.netId == _killerNetId);
		}

		private Entity GetRandomTarget()
		{
			Character character = GetRandom(_entityContainer.GetEntitiesByType<Character>().ToList());

			if (character != null)
			{
				return character;
			}

			return GetRandom(_entityContainer.GetEntitiesByType<SpawnPoint>().ToList());
		}

		private Entity GetNextTarget()
		{
			Character character = SelectNextTarget<Character>();

			if (character != null)
			{
				return character;
			}

			return SelectNextTarget<SpawnPoint>();
		}

		private TEntity SelectNextTarget<TEntity>() where TEntity : Entity
		{
			var entities = _entityContainer.GetEntitiesByType<TEntity>().ToList();

			if (entities.Count == 0)
			{
				return null;
			}

			int currentIndex = _target is TEntity current ? entities.IndexOf(current) : -1;
			return entities[(currentIndex + 1) % entities.Count];
		}

		private static TEntity GetRandom<TEntity>(IReadOnlyList<TEntity> entities) where TEntity : Entity
		{
			return entities.Count == 0 ? null : entities[Random.Range(0, entities.Count)];
		}

		private void Rotate(Vector2 direction)
		{
			float mouseX = direction.x * SensitivityMultiplier * _sensitivity * Time.deltaTime;
			float mouseY = direction.y * SensitivityMultiplier * _sensitivity * Time.deltaTime;
			_yRotation += mouseX;
			_xRotation = Mathf.Clamp(_xRotation - mouseY, -PitchLimit, PitchLimit);
		}

		private void ChangeMouseSettings(MouseSettingsData mouseSettings)
		{
			_sensitivity = mouseSettings.GeneralSensitivity;
		}
	}
}
