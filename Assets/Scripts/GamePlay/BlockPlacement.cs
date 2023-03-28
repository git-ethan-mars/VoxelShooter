using System.Collections.Generic;
using Core;
using Data;
using UnityEngine;

namespace GamePlay
{
    public class BlockPlacement
    {
        private readonly LineRenderer _lineRenderer;
        private readonly Camera _camera;
        private readonly float _placeDistance;

        public BlockPlacement(LineRenderer lineRenderer, Camera camera, float placeDistance)
        {
            _lineRenderer = lineRenderer;
            _camera = camera;
            _placeDistance = placeDistance;
        }
        public void EnableCube()
        {
            _lineRenderer.enabled = true;
            _lineRenderer.positionCount = 0;
        }

        public void DisableCube()
        {
            _lineRenderer.positionCount = 0;
            _lineRenderer.enabled = false;
        }

        public void UpdateCube()
        {
            var raycastResult = GetRayCastHit(out var raycastHit);
            if (!raycastResult)
            {
                _lineRenderer.positionCount = 0;
                return;
            }

            var blockStartPosition = Vector3Int.FloorToInt(raycastHit.point + raycastHit.normal / 2);
            DrawCube(blockStartPosition);
        }

        private void DrawCube(Vector3Int startPosition)
        {
            _lineRenderer.positionCount = 17;
            _lineRenderer.SetPosition(0, startPosition);
            _lineRenderer.SetPosition(1, startPosition + new Vector3Int(1, 0, 0));
            _lineRenderer.SetPosition(2, startPosition + new Vector3Int(1, 1, 0));
            _lineRenderer.SetPosition(3, startPosition + new Vector3Int(0, 1, 0));
            _lineRenderer.SetPosition(4, startPosition);
            _lineRenderer.SetPosition(5, startPosition + new Vector3Int(0, 0, 1));
            _lineRenderer.SetPosition(6, startPosition + new Vector3Int(0, 1, 1));
            _lineRenderer.SetPosition(7, startPosition + new Vector3Int(0, 1, 0));
            _lineRenderer.SetPosition(8, startPosition + new Vector3Int(1, 1, 0));
            _lineRenderer.SetPosition(9, startPosition + new Vector3Int(1, 1, 1));
            _lineRenderer.SetPosition(10, startPosition + new Vector3Int(0, 1, 1));
            _lineRenderer.SetPosition(11, startPosition + new Vector3Int(0, 0, 1));
            _lineRenderer.SetPosition(12, startPosition + new Vector3Int(1, 0, 1));
            _lineRenderer.SetPosition(13, startPosition + new Vector3Int(1, 1, 1));
            _lineRenderer.SetPosition(14, startPosition + new Vector3Int(1, 1, 0));
            _lineRenderer.SetPosition(15, startPosition + new Vector3Int(1, 0, 0));
            _lineRenderer.SetPosition(16, startPosition + new Vector3Int(1, 0, 1));
        }

        public void PlaceBlock(Color32 color)
        {
            var raycastResult = GetRayCastHit(out var raycastHit);
            if (!raycastResult) return;
            GlobalEvents.SendBlockStates(
                new List<Vector3Int>() {Vector3Int.FloorToInt(raycastHit.point + raycastHit.normal / 2)},
                new[] {new BlockData() {Color = color}});
        }

        public void StartDrawBlockLine(Color32 color)
        {
            var raycastResult = GetRayCastHit(out var raycastHit);
            if (!raycastResult) return;
        }

        public void EndDrawBlockLine(Color32 color)
        {
        }

        public void DestroyBlock()
        {
            var raycastResult = GetRayCastHit(out var raycastHit);
            if (!raycastResult) return;
            GlobalEvents.SendBlockStates(
                new List<Vector3Int>() {Vector3Int.FloorToInt(raycastHit.point - raycastHit.normal / 2)},
                new[] {new BlockData() {Color = BlockColor.Empty}});
        }

        private bool GetRayCastHit(out RaycastHit raycastHit)
        {
            var ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            var raycastResult = Physics.Raycast(ray, out raycastHit, _placeDistance);
            if (!raycastResult)
                return false;
            return raycastHit.collider.gameObject.CompareTag("Chunk");
        }
    }
}