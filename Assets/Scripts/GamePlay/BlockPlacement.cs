using UnityEngine;


namespace GamePlay
{
    public class BlockPlacement : MonoBehaviour
    {
        [SerializeField] private float placeDistance;
        [SerializeField] private Camera Camera;

        public void PlaceBlock(byte colorId)
        {
            var ray = Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
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
            var ray = Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
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