using Common;
using UnityEngine;

namespace GamePlay
{
    public class RayCaster
    {
        private readonly Camera _camera;
        private readonly float _placeDistance;

        public RayCaster(Camera mainCamera, float placeDistance)
        {
            _camera = mainCamera;
            _placeDistance = placeDistance;
        }

        public bool GetBuildRayCastHit(out RaycastHit raycastHit)
        {
            return Physics.Raycast(CentredRay, out raycastHit, _placeDistance, LayerMasks.BuildMask);
        }

        public bool GetRayCastHit(out RaycastHit raycastHit, float distance, LayerMask layerMask)
        {
            return Physics.Raycast(CentredRay, out raycastHit, distance, layerMask);
        }

        public Ray CentredRay => _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
    }
}