using System.Linq;
using Mirror;
using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;
using UnityEngine.Animations;

namespace GamePlay
{
	public class Spectator : NetworkBehaviour
	{
		private const float SensitivityMultiplier = 50.0f;
		private const float Speed = 10.0f;
		private const float AccelerationMultiplier = 2.5f;
		private const float DistanceToTarget = 5.0f;

		[SerializeField] private PositionConstraint positionConstraint;

		private IInputService _inputService;
		private CameraProvider _cameraProvider;
		private IStorageService _storageService;
		private EntityContainer _entityContainer;

		private float _sensitivity;
		private float _xRotation;
		private float _yRotation;
		private Transform _target;

		[Inject]
		private void Construct(IInputService inputService, IStorageService storageService, CameraProvider cameraProvider,
			EntityContainer entityContainer)
		{
			_inputService = inputService;
			_cameraProvider = cameraProvider;
			_storageService = storageService;
			_entityContainer = entityContainer;
		}

		public override void OnStartLocalPlayer()
		{
			MountCamera();

			_sensitivity = _storageService.Load<MouseSettingsData>(IStorageService.MouseSettingsKey).GeneralSensitivity;
			_storageService.Subscribe<MouseSettingsData>(ChangeMouseSettings).AddTo(this);
		}

		private void Update()
		{
			if (!isLocalPlayer)
			{
				return;
			}

			Rotate(_inputService.MouseAxis);

			if (_inputService.IsFirstActionButtonDown() || _target == null)
			{
				Entity entity = GetNextTarget();

				if (entity != null)
				{
					_target = entity.transform;
					var constraintSource = new ConstraintSource() { sourceTransform = _target.transform, weight = 1 };

					if (positionConstraint.sourceCount == 0)
					{
						positionConstraint.AddSource(constraintSource);
					}
					else
					{
						positionConstraint.SetSource(0, constraintSource);
					}
				}
			}

			if (_inputService.Axis != Vector2.zero && positionConstraint.sourceCount > 0)
			{
				positionConstraint.RemoveSource(0);
			}

			float speed = _inputService.IsSprintButtonHold() ? Speed * AccelerationMultiplier : Speed;

			transform.position += (transform.forward * _inputService.Axis.x + transform.right * _inputService.Axis.y)
			                      * (speed * Time.deltaTime);
		}

		public override void OnStopLocalPlayer()
		{
			base.OnStopLocalPlayer();

			if (_cameraProvider.MainCamera.transform.parent == transform)
			{
				_cameraProvider.MainCamera.transform.SetParent(null);
			}
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
			TEntity nextTarget = null;

			if (_target == null)
			{
				nextTarget = _entityContainer.GetEntitiesByType<TEntity>().FirstOrDefault();
			}
			else
			{
				bool previousEntityWasTarget = false;

				foreach (TEntity entity in _entityContainer.GetEntitiesByType<TEntity>())
				{
					if (previousEntityWasTarget)
					{
						nextTarget = entity;
						break;
					}

					if (entity.transform == _target)
					{
						previousEntityWasTarget = true;
					}
				}

				if (nextTarget == null)
				{
					nextTarget = _entityContainer.GetEntitiesByType<TEntity>().FirstOrDefault();
				}
			}

			return nextTarget;
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

		private void MountCamera()
		{
			_cameraProvider.MainCamera.transform.SetParent(transform);
			_cameraProvider.MainCamera.transform.SetLocalPositionAndRotation(Vector3.back * DistanceToTarget, Quaternion.identity);
		}
	}
}
