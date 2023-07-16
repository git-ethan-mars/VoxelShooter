using UnityEngine;

namespace Rendering
{
    public class Raycaster
    {
        private readonly Camera _camera;
        private readonly float _placeDistance;

        public Raycaster(Camera mainCamera)
        {
            _camera = mainCamera;
        }

        public bool GetRayCastHit(out RaycastHit raycastHit, float distance, LayerMask layerMask)
        {
            return Physics.Raycast(CentredRay, out raycastHit, distance, layerMask);
        }

        public Ray CentredRay => _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
    }
}