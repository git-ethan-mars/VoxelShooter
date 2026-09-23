using Mirror;
using Reflex.Attributes;
using UnityEngine;
namespace GamePlay
{
	public class CharacterAnimation : NetworkBehaviour
	{
		private const float RotationSpineThreshold = 30f;
		
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
				headTrackingObject.position = _cameraProvider.MainCamera.transform.position + _cameraProvider.MainCamera.transform.forward;
				HandleSpineRotation();
			}
		}
		
		private void HandleSpineRotation()
		{
			var currentAngle = _cameraProvider.MainCamera.transform.eulerAngles.y;
			
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
				spineTrackingObject.position = Vector3.Slerp(spineTrackingObject.position, headTrackingObject.position, 0.05f);
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