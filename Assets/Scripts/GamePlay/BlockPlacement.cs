using System;
using UnityEngine;

// ReSharper disable PossibleLossOfFraction

namespace GamePlay
{
    public class BlockPlacement : MonoBehaviour
    {
        [SerializeField] private float placeDistance;
        private Block _currentBlock;
        private Camera Camera { get; set; }

        private void Awake()
        {
            GlobalEvents.OnBlockChoiceEvent.AddListener(newBlock => _currentBlock = newBlock);
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
                GlobalEvents.SendBlockState(_currentBlock, Vector3Int.FloorToInt(hitInfo.point + hitInfo.normal / 2));
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