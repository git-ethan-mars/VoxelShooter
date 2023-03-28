using System.Collections.Generic;
using Core;
using Data;
using UnityEngine;

namespace GamePlay
{
    public class ColoringBrush
    {
        private readonly float _placeDistance;
        private readonly Camera _camera;

        public ColoringBrush(Camera camera, float placeDistance)
        {
            _camera = camera;
            _placeDistance = placeDistance;
        }

        public void PaintBlock(Color32 color)
        {
            var ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            var raycastResult = Physics.Raycast(ray, out var hitInfo, _placeDistance);
            if (!raycastResult) return;
            if (hitInfo.collider.CompareTag("Chunk"))
            {
                GlobalEvents.SendBlockStates(
                    new List<Vector3Int>() {Vector3Int.FloorToInt(hitInfo.point - hitInfo.normal / 2)},
                    new[] {new BlockData {Color = color}});
            }
        }
    }
}