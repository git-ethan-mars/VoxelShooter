using Data;
using Infrastructure.AssetManagement;
using Networking.Synchronization;
using Rendering;
using UI;
using UnityEngine;

namespace Inventory
{
    public class SpawnPointView : IInventoryItemView, ILeftMouseButtonDownHandler, IUpdated
    {
        public Sprite Icon { get; }
        private readonly CubeRenderer _cubeRenderer;
        private readonly MapSynchronization _mapSynchronization;

        public SpawnPointView(CubeRenderer cubeRenderer, MapSynchronization mapSynchronization, SpawnPointItem item)
        {
            _cubeRenderer = cubeRenderer;
            _mapSynchronization = mapSynchronization;
            Icon = item.inventoryIcon;
        }

        public void Select()
        {
            _cubeRenderer.EnableCube();
        }

        public void Unselect()
        {
            _cubeRenderer.DisableCube();
        }

        public void OnLeftMouseButtonDown()
        {
            CreateSpawnPoint();
            Debug.Log("Спавн поинт добавлен");
        }

        public void InnerUpdate()
        {
            _cubeRenderer.UpdateCube();
        }

        private void CreateSpawnPoint()
        {
            var raycastResult = _cubeRenderer.GetRayCastHit(out var raycastHit);
            if (!raycastResult) return;
            _mapSynchronization.CreateSpawnPoint(Vector3Int.FloorToInt(raycastHit.point - raycastHit.normal / 2));
        }
    }
}