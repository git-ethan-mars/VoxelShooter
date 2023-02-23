using UnityEngine;

namespace GamePlay
{
    public class ColoringBrush : MonoBehaviour
    {
        [SerializeField] private float placeDistance;
        [SerializeField] private Camera Camera;

        public void PaintBlock(byte colorId)
        {
            var ray = Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            var raycastResult = Physics.Raycast(ray, out var hitInfo, placeDistance);
            if (!raycastResult) return;
            if (hitInfo.collider.CompareTag("Chunk"))
            {
                GlobalEvents.SendBlockState(new Block {ColorID = colorId},
                    Vector3Int.FloorToInt(hitInfo.point - hitInfo.normal / 2));
            }
        }
    }
}