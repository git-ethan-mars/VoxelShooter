using DG.Tweening;
using UnityEngine;

namespace GamePlay
{
	public class CameraProvider
	{
		private const float DefaultFov = 60;
		private const float ZoomEnteringTime = 0.2f;

		public Camera MainCamera { get; } = Camera.main;
		public Ray CentredRay => MainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
		public bool IsZoomed { get; private set; }

		public void ZoomIn(float zoomMultiplier)
		{
			DOTween.To(() => MainCamera.fieldOfView, x => MainCamera.fieldOfView = x, DefaultFov / zoomMultiplier, ZoomEnteringTime);
			IsZoomed = true;
		}

		public void ZoomOut()
		{
			DOTween.To(() => MainCamera.fieldOfView, x => MainCamera.fieldOfView = x, DefaultFov, ZoomEnteringTime);
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