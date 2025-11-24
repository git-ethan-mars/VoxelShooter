using System;
using Mirror;
using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;
namespace GamePlay
{
	public class CharacterCamera : NetworkBehaviour
	{
		private const float RotationLimit = 89.9f;
		private const float SensitivityMultiplier = 50.0f;

		[SerializeField] private Transform head;
		
		private IInputService _inputService;
		private IStorageService _storageService;
		private CameraProvider _cameraProvider;

		private float _xRotation;
		private float _yRotation;
		private float _mouseSensitivity;
		private float _aimSensitivity;

		[Inject]
		private void Construct(IInputService inputService, IStorageService storageService, CameraProvider cameraProvider)
		{
			_inputService = inputService;
			_storageService = storageService;
			_cameraProvider = cameraProvider;
		}

		public override void OnStartLocalPlayer()
		{
			base.OnStartLocalPlayer();

			var mouseSettings = _storageService.Load<MouseSettingsData>(IStorageService.MouseSettingsKey);
			_mouseSensitivity = mouseSettings.GeneralSensitivity;
			_aimSensitivity = mouseSettings.AimSensitivity;
			
			_storageService.Subscribe<MouseSettingsData>(OnMouseSettingsChanged)
				.AddTo(this);
		}

		private void Update()
		{
			if (isLocalPlayer)
			{
				_cameraProvider.MainCamera.transform.position = head.position;
				Rotate(_inputService.MouseAxis);
			}
		}

		private void Rotate(Vector2 direction)
		{
			float sensitivity = _cameraProvider.IsZoomed ? _aimSensitivity : _mouseSensitivity;
			float mouseX = direction.x * SensitivityMultiplier * sensitivity * Time.deltaTime;
			float mouseY = direction.y * SensitivityMultiplier * sensitivity * Time.deltaTime;
			_yRotation += mouseX;
			_xRotation -= mouseY;
			_xRotation = Math.Clamp(_xRotation, -RotationLimit, RotationLimit);
			
			_cameraProvider.MainCamera.transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0.0f);
		}
		
		private void OnMouseSettingsChanged(MouseSettingsData mouseSettings)
		{
			_mouseSensitivity = mouseSettings.GeneralSensitivity;
			_aimSensitivity = mouseSettings.AimSensitivity;
		}
	}
}