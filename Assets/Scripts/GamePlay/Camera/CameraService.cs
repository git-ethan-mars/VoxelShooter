using UnityEngine;
using UnityEngine.Animations;

namespace GamePlay
{
	public class CameraService
	{
		private const float DistanceToTarget = 5.0f;
		private const float DefaultFov = 60;

		public Camera MainCamera { get; private set; } = Camera.main;
		public Ray CentredRay => MainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
		public bool IsZoomed { get; private set; }

		public void ZoomIn(float zoomMultiplier)
		{
			MainCamera.fieldOfView = DefaultFov / zoomMultiplier;
			IsZoomed = true;
		}

		public void ZoomOut()
		{
			MainCamera.fieldOfView = DefaultFov;
			IsZoomed = false;
		}

		public bool GetBuildRayCastHit(out RaycastHit raycastHit, float distance)
		{
			return Physics.Raycast(CentredRay, out raycastHit, distance, LayerMasks.BuildMask);
		}

		public bool GetRayCastHit(out RaycastHit raycastHit, float distance, LayerMask layerMask)
		{
			return Physics.Raycast(CentredRay, out raycastHit, distance, layerMask);
		}
	}
}