using Mirror;
using Reflex.Attributes;
using UnityEngine;

namespace GamePlay
{
	public class CharacterAnimation : NetworkBehaviour
	{
		private const float RotationSpineThreshold = 30f;

		// Matches the Head IK X limit. Near ±90° the horizontal part of the look direction vanishes
		// and the Head IK Y aim flips the head around, so the target never goes steeper than this.
		private const float HeadPitchLimit = 80f;

		// A distant target keeps the aim stable against small offsets between the head and the camera.
		private const float HeadTargetDistance = 10f;

		[SerializeField] private CharacterMovement characterMovement;
		[SerializeField] private Transform headTrackingObject;
		[SerializeField] private Transform spineTrackingObject;

		private CameraProvider _cameraProvider;

		private float _leftBoarderAngle = -RotationSpineThreshold;
		private float _rightBoarderAngle = RotationSpineThreshold;
		private float _previousAngle;
		private int _totalTurns;

		[Inject]
		private void Construct(CameraProvider cameraProvider)
		{
			_cameraProvider = cameraProvider;
		}

		private void Update()
		{
			if (isLocalPlayer)
			{
				Transform cameraTransform = _cameraProvider.MainCamera.transform;
				Vector3 lookDirection = GetClampedLookDirection(cameraTransform);
				headTrackingObject.position = cameraTransform.position + lookDirection * HeadTargetDistance;
				HandleSpineRotation(cameraTransform.position + lookDirection);
			}
		}

		private static Vector3 GetClampedLookDirection(Transform cameraTransform)
		{
			Vector3 euler = cameraTransform.eulerAngles;
			float pitch = Mathf.Clamp(Mathf.DeltaAngle(0f, euler.x), -HeadPitchLimit, HeadPitchLimit);
			return Quaternion.Euler(pitch, euler.y, 0f) * Vector3.forward;
		}

		private void HandleSpineRotation(Vector3 lookTarget)
		{
			float currentAngle = _cameraProvider.MainCamera.transform.eulerAngles.y;

			if (currentAngle - _previousAngle < -180)
			{
				_totalTurns += 1;
			}

			if (currentAngle - _previousAngle > 180)
			{
				_totalTurns -= 1;
			}

			float totalAngle = currentAngle + _totalTurns * 360;

			if (characterMovement.GetHorizontalVelocity().magnitude != 0)
			{
				spineTrackingObject.position = Vector3.Slerp(spineTrackingObject.position, lookTarget, 0.05f);
				_leftBoarderAngle = totalAngle - RotationSpineThreshold;
				_rightBoarderAngle = totalAngle + RotationSpineThreshold;
			}
			else
			{
				if (totalAngle < _leftBoarderAngle)
				{
					_leftBoarderAngle = totalAngle;
					_rightBoarderAngle = _leftBoarderAngle + 2 * RotationSpineThreshold;
				}

				if (totalAngle > _rightBoarderAngle)
				{
					_rightBoarderAngle = totalAngle;
					_leftBoarderAngle = _rightBoarderAngle - 2 * RotationSpineThreshold;
				}

				spineTrackingObject.position = Vector3.Lerp(spineTrackingObject.position, transform.position +
				                                                                          Quaternion.Euler(0, (_leftBoarderAngle + _rightBoarderAngle) / 2, 0) * Vector3.forward, 0.1f);
			}

			_previousAngle = currentAngle;
		}
	}
}
