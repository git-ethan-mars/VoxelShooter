using Core;
using UnityEngine;

namespace GamePlay
{
    public class BlockPlacement : MonoBehaviour
    {
        [SerializeField] private float placeDistance;
        private LineRenderer _lineRenderer;
        private Camera _camera;

        private void OnEnable()
        {
            _lineRenderer ??= GetComponent<LineRenderer>();
            _lineRenderer.positionCount = 0;
        }

        private void OnDisable()
        {
            _lineRenderer.positionCount = 0;
        }

        private void Update()
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
            GlobalEvents.SendBlockState(new Block {Color = color},
                Vector3Int.FloorToInt(raycastHit.point + raycastHit.normal / 2));
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
            var destroyedBlock = new Block() {Color = BlockColor.Empty};
            GlobalEvents.SendBlockState(destroyedBlock,
                Vector3Int.FloorToInt(raycastHit.point - raycastHit.normal / 2));
        }

        private bool GetRayCastHit(out RaycastHit raycastHit)
        {
            _camera ??= gameObject.GetComponentInChildren<Camera>();
            var ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            var raycastResult = Physics.Raycast(ray, out raycastHit, placeDistance);
            if (!raycastResult)
                return false;
            return raycastHit.collider.gameObject.CompareTag("Chunk");
        }
    }
}