using UnityEngine;

namespace Rendering
{
    public class Raycaster
    {
        private readonly Camera _camera;
        private readonly float _placeDistance;
        private const string ChunkTag = "Chunk";

        public Raycaster(Camera mainCamera, float placeDistance)
        {
            _camera = mainCamera;
            _placeDistance = placeDistance;
        }
        
        public bool GetRayCastHit(out RaycastHit raycastHit)
        {
            var ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            var raycastResult = Physics.Raycast(ray, out raycastHit, _placeDistance);
            return raycastResult && raycastHit.collider.gameObject.CompareTag(ChunkTag);
        }
    }
}