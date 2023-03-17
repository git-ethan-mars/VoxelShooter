using System.Collections.Generic;
using Core;
using UnityEngine;

namespace GamePlay
{
    public class ColoringBrush : MonoBehaviour
    {
        [SerializeField] private float placeDistance;
        private Camera _camera;

        public void PaintBlock(Color32 color)
        {
            _camera ??= gameObject.GetComponentInChildren<Camera>();
            var ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            var raycastResult = Physics.Raycast(ray, out var hitInfo, placeDistance);
            if (!raycastResult) return;
            if (hitInfo.collider.CompareTag("Chunk"))
            {
                GlobalEvents.SendBlockStates(
                    new List<Vector3Int>() {Vector3Int.FloorToInt(hitInfo.point - hitInfo.normal / 2)},
                    new[] {new Block {Color = color}});
            }
        }
    }
}