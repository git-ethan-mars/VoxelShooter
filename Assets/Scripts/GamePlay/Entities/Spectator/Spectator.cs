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
		private const float DistanceToPlayer = 5.0f;
		
		[SerializeField]
		private PositionConstraint positionConstraint;
		
		private IInputService _inputService;
		private CameraProvider _cameraProvider;
		private IStorageService _storageService;
		private EntityContainerService _entityContainer;

		private float _sensitivity;
		private float _xRotation;
		private float _yRotation;
		private Character _target;

		[Inject]
		private void Construct(IInputService inputService, IStorageService storageService, CameraProvider cameraProvider,
			EntityContainerService entityContainer)
		{
			_inputService = inputService;
			_cameraProvider = cameraProvider;
			_storageService = storageService;
			_entityContainer = entityContainer;
		}

		public override void OnStartLocalPlayer()
		{
			base.OnStartLocalPlayer();
			
			MountCamera();
			
			_sensitivity = _storageService.Load<MouseSettingsData>(IStorageService.MouseSettingsKey).GeneralSensitivity;
			_storageService.Subscribe<MouseSettingsData>(ChangeMouseSettings).AddTo(this);
		}

		public override void OnStopLocalPlayer()
		{
			base.OnStopLocalPlayer();
			
			if (_cameraProvider.MainCamera.transform.parent == transform)
			{
				_cameraProvider.MainCamera.transform.SetParent(null);
			}
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
				_target = SelectNextTarget();

				if (_target == null)
				{
					return;
				}
				
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

		private Character SelectNextTarget()
		{
			Character nextTarget = null;

			if (_target == null)
			{
				nextTarget = _entityContainer.GetEntitiesByType<Character>().FirstOrDefault();
			}
			else
			{
				var previousCharacterWasTarget = false;

				foreach (var character in _entityContainer.GetEntitiesByType<Character>())
				{
					if (previousCharacterWasTarget)
					{
						nextTarget = character;
						break;
					}

					if (character == _target)
					{
						previousCharacterWasTarget = true;
					}
				}
				
				if (nextTarget == null)
				{
					nextTarget = _entityContainer.GetEntitiesByType<Character>().First();
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
			_cameraProvider.MainCamera.transform.SetLocalPositionAndRotation(Vector3.back * DistanceToPlayer, Quaternion.identity);
		}
	}
}