using System;
using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;
namespace GamePlay
{
	public class CharacterMovement : MonoBehaviour, IMovement
	{
		private const float GravityScale = 3;
		private const float GroundDistanceThreshold = 1e-3f;
		private const float AccelerationTime = 0.5f;

		private const float RotationLimit = 87f;
		private const float RotationBodyThreshold = 30f;
		private const float SensitivityMultiplier = 50.0f;

		[SerializeField] private Rigidbody playerRigidbody;
		[SerializeField] private CapsuleCollider hitBox;
		[SerializeField] private Transform headTrackingObject;
		[SerializeField] private Transform bodyTrackingObject;
		[SerializeField] private Transform head;

		private static readonly Vector3 HorizontalMask = new Vector3(1, 0, 1);

		private IStorageService _storageService;

		private Vector3 _desiredMovementDirection;
		private float _jumpHeight;
		private bool _jumpRequested;
		private float _speed;
		private bool _resetHorizontalVelocity;

		private float _xRotation;
		private float _yRotation;
		private float _mouseSensitivity;
		private float _leftBoarderAngle;
		private float _rightBoarderAngle;

		[Inject]
		private void Construct(IStorageService storageService)
		{
			_storageService = storageService;
		}

		public void Initialize()
		{
			var mouseSettings = _storageService.Load<MouseSettingsData>(IStorageService.MouseSettingsKey);
			_mouseSensitivity = mouseSettings.GeneralSensitivity;
			_leftBoarderAngle = -RotationBodyThreshold;
			_rightBoarderAngle = RotationBodyThreshold;
			//_aimSensitivity = mouseSettings.AimSensitivity;
			_storageService.Subscribe<MouseSettingsData>(OnMouseSettingsChanged).AddTo(this);
		}

		public void Move(Vector2 direction, float speed)
		{
			Vector3 forward = Vector3.ProjectOnPlane(headTrackingObject.position - transform.position, Vector3.up);
			Vector3 right = Quaternion.Euler(0f, 90, 0f) * forward;
			Vector3 horizontalDirection = (direction.x * forward + direction.y * right).normalized;

			if (Vector3.Dot(_desiredMovementDirection, horizontalDirection) <= 0)
			{
				_resetHorizontalVelocity = true;
			}

			_desiredMovementDirection = horizontalDirection;
			_speed = speed;
		}

		public void Jump(float jumpHeight)
		{
			_jumpRequested = true;
			_jumpHeight = jumpHeight;
		}

		public void Rotate(Vector2 direction)
		{
			float mouseX = direction.x * SensitivityMultiplier * _mouseSensitivity * Time.deltaTime;
			float mouseY = direction.y * SensitivityMultiplier * _mouseSensitivity * Time.deltaTime;
			_yRotation += mouseX;
			_xRotation -= mouseY;
			_xRotation = Math.Clamp(_xRotation, -RotationLimit, RotationLimit);

			headTrackingObject.position = Quaternion.Euler(_xRotation, _yRotation, 0) * transform.forward + head.position;

			if (_desiredMovementDirection.magnitude != 0)
			{
				bodyTrackingObject.position = Vector3.Slerp(bodyTrackingObject.position, headTrackingObject.position, 0.05f);
				_leftBoarderAngle = _yRotation - RotationLimit;
				_rightBoarderAngle = _yRotation + RotationLimit;
			}
			else
			{
				bodyTrackingObject.localPosition = Vector3.Slerp(bodyTrackingObject.localPosition, Quaternion.Euler(0,
					(_leftBoarderAngle + _rightBoarderAngle) /
					2, 0) * transform
					.forward, 0.2f);
				
				if (_yRotation < _leftBoarderAngle)
				{
					_leftBoarderAngle = _yRotation;
					_rightBoarderAngle = _leftBoarderAngle + 2 * RotationBodyThreshold;
				}
				if (_yRotation > _rightBoarderAngle)
				{
					_rightBoarderAngle = _yRotation;
					_leftBoarderAngle = _rightBoarderAngle - 2 * RotationBodyThreshold;
				}
			}
		}

		public void Tick()
		{
			if (IsGrounded())
			{
				if (_jumpRequested)
				{
					playerRigidbody.AddForce(Mathf.Sqrt(-2 * GravityScale * Physics.gravity.y * _jumpHeight) * Vector3.up,
						ForceMode.Impulse);
				}
			}
			else
			{
				playerRigidbody.AddForce(GravityScale * Physics.gravity);
			}

			playerRigidbody.AddForce(-GetHorizontalVelocity() / Time.deltaTime);

			if (!_resetHorizontalVelocity)
			{
				playerRigidbody.AddForce(_desiredMovementDirection * GetHorizontalVelocity().magnitude / Time.deltaTime);
				playerRigidbody.AddForce(
					Math.Min((_speed - GetHorizontalVelocity().magnitude) / Time.deltaTime,
						_speed / AccelerationTime) * _desiredMovementDirection);
			}

			_jumpRequested = false;
			_resetHorizontalVelocity = false;
		}

		public Vector3 ForwardVector => Vector3.ProjectOnPlane(headTrackingObject.position - transform.position, Vector3.up);

		private Vector3 GetHorizontalVelocity()
		{
			return Vector3.Scale(HorizontalMask, playerRigidbody.linearVelocity);
		}

		private bool IsGrounded()
		{
			bool isGrounded = Physics.CheckBox(playerRigidbody.position,
				new Vector3(hitBox.radius / 2, GroundDistanceThreshold, hitBox.radius / 2),
				Quaternion.identity, LayerMasks.BuildMask);
			return isGrounded;
		}

		private void OnMouseSettingsChanged(MouseSettingsData mouseSettings)
		{
			_mouseSensitivity = mouseSettings.GeneralSensitivity;
			//_aimSensitivity = mouseSettings.AimSensitivity;
		}
	}
}