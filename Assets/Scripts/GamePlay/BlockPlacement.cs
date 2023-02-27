using Mirror;
using UnityEngine;

namespace GamePlay
{
    public class BlockPlacement : MonoBehaviour
    {
        [SerializeField] private float placeDistance;
        private Camera _camera;

        public void PlaceBlock(byte colorId)
        {
            _camera ??= gameObject.GetComponentInChildren<Camera>();
            var ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            var raycastResult = Physics.Raycast(ray, out var hitInfo, placeDistance);
            if (!raycastResult) return;
            if (hitInfo.collider.CompareTag("Chunk") || hitInfo.collider.gameObject.CompareTag("Floor"))
            {
                GlobalEvents.SendBlockState(new Block {ColorID = colorId},
                    Vector3Int.FloorToInt(hitInfo.point + hitInfo.normal / 2));
            }
        }

        public void DestroyBlock()
        {
            _camera ??= gameObject.GetComponentInChildren<Camera>();
            var ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            var raycastResult = Physics.Raycast(ray, out var hitInfo, placeDistance);
            if (!raycastResult) return;
            if (hitInfo.collider.gameObject.CompareTag("Chunk"))
            {
                var destroyedBlock = new Block() {ColorID = BlockColor.Empty};
                GlobalEvents.SendBlockState(destroyedBlock, Vector3Int.FloorToInt(hitInfo.point - hitInfo.normal / 2));
            }
        }
    }
}