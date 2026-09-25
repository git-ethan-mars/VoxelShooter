using System.Collections.Generic;
using System.Linq;
using Data;
using Mirror;
using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;
using UnityEngine.Animations;
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

		[SerializeField] private PositionConstraint positionConstraint;
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
			else
			{
				UpdateFollowCameraDistance();
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

			var constraintSource = new ConstraintSource() { sourceTransform = target.transform, weight = 1 };

			if (positionConstraint.sourceCount == 0)
			{
				positionConstraint.AddSource(constraintSource);
			}
			else
			{
				positionConstraint.SetSource(0, constraintSource);
			}

			// The constraint moves the spectator now, so physics must not fight it.
			if (!rigidBody.isKinematic)
			{
				rigidBody.linearVelocity = Vector3.zero;
				rigidBody.isKinematic = true;
			}
			positionConstraint.constraintActive = true;
			UpdateFollowCameraDistance();
		}

		private void EnterFreeView()
		{
			Vector3 cameraPosition = _cameraProvider.MainCamera.transform.position;

			_mode = SpectatingMode.FreeView;
			_target = null;

			if (positionConstraint.sourceCount > 0)
			{
				positionConstraint.RemoveSource(0);
			}

			positionConstraint.constraintActive = false;
			Vector3 position = ClampInsideWalls(cameraPosition);
			transform.position = position;
			rigidBody.position = position;
			rigidBody.isKinematic = false;
			rigidBody.linearVelocity = Vector3.zero;
			_cameraProvider.MainCamera.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		}

		private void Move(Vector2 direction)
		{
			float speed = _inputService.IsSprintButtonHold() ? Speed * AccelerationMultiplier : Speed;

			// Moved by velocity, so the map walls stop the spectator.
			rigidBody.linearVelocity = (transform.forward * direction.x + transform.right * direction.y) * speed;
		}

		// Pulls the follow camera closer when a map wall is behind it, so it never looks out of the map.
		private void UpdateFollowCameraDistance()
		{
			float distance = DistanceToTarget;

			if (Physics.SphereCast(transform.position, CameraRadius, -transform.forward, out RaycastHit hit, DistanceToTarget,
				    LayerMasks.WallMask, QueryTriggerInteraction.Ignore))
			{
				distance = hit.distance;
			}

			_cameraProvider.MainCamera.transform.SetLocalPositionAndRotation(Vector3.back * distance, Quaternion.identity);
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
			_xRotation -= mouseY;
			transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
		}

		private void ChangeMouseSettings(MouseSettingsData mouseSettings)
		{
			_sensitivity = mouseSettings.GeneralSensitivity;
		}
	}
}
