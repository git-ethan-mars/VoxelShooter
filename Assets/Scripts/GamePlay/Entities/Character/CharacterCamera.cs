using Mirror;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Animations;
namespace GamePlay
{
	public class CharacterCamera : NetworkBehaviour
	{
		[SerializeField] private Transform headTrackingObject;
		[SerializeField] private Transform headPosition;

		private CameraService _cameraService;
		private LookAtConstraint _cameraConstraint;

		[Inject]
		private void Construct(CameraService cameraService)
		{
			_cameraService = cameraService;
			_cameraConstraint = _cameraService.MainCamera.GetComponent<LookAtConstraint>();
		}

		public override void OnStartLocalPlayer()
		{
			base.OnStartLocalPlayer();

			SetupCamera();
		}

		public override void OnStopLocalPlayer()
		{
			base.OnStopLocalPlayer();
			
			if (_cameraService.MainCamera.transform.parent == transform)
			{
				_cameraService.MainCamera.transform.SetParent(null);
			}
		}

		private void SetupCamera()
		{
			Transform cameraTransform = _cameraService.MainCamera.transform;
			cameraTransform.SetParent(transform);
			cameraTransform.transform.position = headPosition.position;
			_cameraConstraint.AddSource(new ConstraintSource { sourceTransform = headTrackingObject, weight = 1 });
		}
	}
}