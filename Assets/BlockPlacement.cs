using System.Collections.Generic;
using UnityEngine;

// ReSharper disable PossibleLossOfFraction

public class BlockPlacement : MonoBehaviour
{
    [SerializeField] private List<GameObject> blocks;
    [SerializeField] private float placeDistance;
    private Camera Camera { get; set; }
    private Vector2 CenterPosition { get; set; }
    private float BlockSize { get; set; }

    private void Start()
    {
        GetComponent<Transform>();
        Camera = Camera.main;
        BlockSize = blocks[0].GetComponent<BoxCollider>().size.x;
        CenterPosition = new Vector2(Screen.width / 2, Screen.height / 2);
    }

    private void Update()
    {
        var ray = Camera.ScreenPointToRay(CenterPosition);
        var raycastResult = Physics.Raycast(ray, out var hitInfo, placeDistance);
        if (Input.GetMouseButtonDown(1) && raycastResult)
        {
            if (hitInfo.collider.transform.position.x - hitInfo.point.x >= BlockSize / 2)
            {
                var newBlockPosition = hitInfo.collider.transform.position - new Vector3(BlockSize, 0, 0);
                Instantiate(blocks[0], newBlockPosition, Quaternion.identity);
            }

            else if (hitInfo.collider.transform.position.x - hitInfo.point.x <= -BlockSize / 2)
            {
                var newBlockPosition = hitInfo.collider.transform.position + new Vector3(BlockSize, 0, 0);
                Instantiate(blocks[0], newBlockPosition, Quaternion.identity);
            }

            else if (hitInfo.collider.transform.position.y - hitInfo.point.y >= BlockSize / 2)
            {
                var newBlockPosition = hitInfo.collider.transform.position - new Vector3(0, BlockSize, 0);
                Instantiate(blocks[0], newBlockPosition, Quaternion.identity);
            }

            else if (hitInfo.collider.transform.position.y - hitInfo.point.y <= -BlockSize / 2)
            {
                var newBlockPosition = hitInfo.collider.transform.position + new Vector3(0, BlockSize, 0);
                Instantiate(blocks[0], newBlockPosition, Quaternion.identity);
            }

            else if (hitInfo.collider.transform.position.z - hitInfo.point.z >= BlockSize / 2)
            {
                var newBlockPosition = hitInfo.collider.transform.position - new Vector3(0, 0, BlockSize);
                Instantiate(blocks[0], newBlockPosition, Quaternion.identity);
            }

            else if (hitInfo.collider.transform.position.z - hitInfo.point.z <= -BlockSize / 2)
            {
                var newBlockPosition = hitInfo.collider.transform.position + new Vector3(0, 0, BlockSize);
                Instantiate(blocks[0], newBlockPosition, Quaternion.identity);
            }
        }
    }
}