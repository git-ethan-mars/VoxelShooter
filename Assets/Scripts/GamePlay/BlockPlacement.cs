using System;
using UI;
using UnityEngine;

// ReSharper disable PossibleLossOfFraction

namespace GamePlay
{
    public class BlockPlacement : MonoBehaviour
    {
        [SerializeField] private float placeDistance;
        private InventoryItem _currentItem;
        private Camera Camera { get; set; }

        private void Awake()
        {
            GlobalEvents.OnSlotChangeEvent.AddListener(newItem => _currentItem = newItem);
        }

        private void Start()
        {
            Camera = Camera.main;
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(0)) return;
            var ray = Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            var raycastResult = Physics.Raycast(ray, out var hitInfo, placeDistance);
            if (!raycastResult) return;
            if (Input.GetMouseButtonDown(1) &&
                (hitInfo.collider.CompareTag("Chunk") || hitInfo.collider.gameObject.CompareTag("Floor")))
            {
                GlobalEvents.SendBlockState(new Block() {Kind = _currentItem.kind},
                    Vector3Int.FloorToInt(hitInfo.point + hitInfo.normal / 2));
            }

            if (Input.GetMouseButtonDown(0) && hitInfo.collider.gameObject.CompareTag("Chunk"))
            {
                var destroyedBlock = new Block() {Kind = BlockKind.Empty};
                GlobalEvents.SendBlockState(destroyedBlock, Vector3Int.FloorToInt(hitInfo.point - hitInfo.normal / 2));
            }

            if (Input.GetMouseButtonDown(0) && hitInfo.collider.gameObject.CompareTag("TNT"))
            {
                var cube = hitInfo.collider.gameObject;
                cube.GetComponent<Explosion>().Explode();
            }
        }
    }
}