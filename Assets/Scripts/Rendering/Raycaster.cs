using UnityEngine;

namespace Rendering
{
    public class Raycaster
    {
        public readonly Camera Camera;
        public readonly float PlaceDistance;
        private const string ChunkTag = "Chunk";

        public Raycaster(Camera mainCamera, float placeDistance)
        {
            Camera = mainCamera;
            PlaceDistance = placeDistance;
        }
        
        public bool GetRayCastHit(out RaycastHit raycastHit)
        {
            var ray = Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            var raycastResult = Physics.Raycast(ray, out raycastHit, PlaceDistance);
            return raycastResult && raycastHit.collider.gameObject.CompareTag(ChunkTag);
        }
    }
}