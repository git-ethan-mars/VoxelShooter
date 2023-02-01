using System.Collections.Generic;
using UnityEngine;

// ReSharper disable PossibleLossOfFraction

public class BlockPlacement : MonoBehaviour
{
    [SerializeField] private List<GameObject> blocks;
    [SerializeField] private GameObject slicedBlock;
    [SerializeField] private float placeDistance;
    [SerializeField] private float explosionForce;
    [SerializeField] private float explosionRadius;
    [SerializeField] private string RomanChe;
    private Block[] _blocks;
    private Camera Camera { get; set; }
    private Vector2 CenterPosition { get; set; }
    private float BlockSize { get; set; }

    public void Awake()
    {
        _blocks = LevelReader.ReadLevel(RomanChe);
        for (var i = 0; i < Level.Width; i++)
        {
            for (var j = 0; j < Level.Height; j++)
            {
                for (var k = 0; k < Level.Depth; k++)
                {
                    var block = _blocks[i * Level.Height * Level.Depth + j * Level.Depth + k];
                    if (block.color == 0)
                        continue;
                    Instantiate(blocks[0], new Vector3(i, j, k), Quaternion.identity);
                }
            }
        }
    }

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
        if (!raycastResult) return;
        if (Input.GetMouseButtonDown(1) && hitInfo.collider.CompareTag("Block"))
        {
            if (hitInfo.collider.transform.position.x - hitInfo.point.x >= BlockSize / 2)
            {
                var newBlockPosition = hitInfo.collider.transform.position - new Vector3(BlockSize, 0, 0);
                Instantiate(blocks[0], newBlockPosition, Quaternion.identity);
                _blocks[(int)newBlockPosition.x * Level.Height * Level.Depth + (int)newBlockPosition.y * Level.Depth + (int)newBlockPosition.z].color = 1;
                _blocks[(int)newBlockPosition.x * Level.Height * Level.Depth + (int)newBlockPosition.y * Level.Depth + (int)newBlockPosition.z].isInvincible = false;
            }

            else if (hitInfo.collider.transform.position.x - hitInfo.point.x <= -BlockSize / 2)
            {
                var newBlockPosition = hitInfo.collider.transform.position + new Vector3(BlockSize, 0, 0);
                Instantiate(blocks[0], newBlockPosition, Quaternion.identity);
                _blocks[(int)newBlockPosition.x * Level.Height * Level.Depth + (int)newBlockPosition.y * Level.Depth + (int)newBlockPosition.z].color = 1;
                _blocks[(int)newBlockPosition.x * Level.Height * Level.Depth + (int)newBlockPosition.y * Level.Depth + (int)newBlockPosition.z].isInvincible = false;
            }

            else if (hitInfo.collider.transform.position.y - hitInfo.point.y >= BlockSize / 2)
            {
                var newBlockPosition = hitInfo.collider.transform.position - new Vector3(0, BlockSize, 0);
                Instantiate(blocks[0], newBlockPosition, Quaternion.identity);
                _blocks[(int)newBlockPosition.x * Level.Height * Level.Depth + (int)newBlockPosition.y * Level.Depth + (int)newBlockPosition.z].color = 1;
                _blocks[(int)newBlockPosition.x * Level.Height * Level.Depth + (int)newBlockPosition.y * Level.Depth + (int)newBlockPosition.z].isInvincible = false;
            }

            else if (hitInfo.collider.transform.position.y - hitInfo.point.y <= -BlockSize / 2)
            {
                var newBlockPosition = hitInfo.collider.transform.position + new Vector3(0, BlockSize, 0);
                Instantiate(blocks[0], newBlockPosition, Quaternion.identity);
                _blocks[(int)newBlockPosition.x * Level.Height * Level.Depth + (int)newBlockPosition.y * Level.Depth + (int)newBlockPosition.z].color = 1;
                _blocks[(int)newBlockPosition.x * Level.Height * Level.Depth + (int)newBlockPosition.y * Level.Depth + (int)newBlockPosition.z].isInvincible = false;
            }

            else if (hitInfo.collider.transform.position.z - hitInfo.point.z >= BlockSize / 2)
            {
                var newBlockPosition = hitInfo.collider.transform.position - new Vector3(0, 0, BlockSize);
                Instantiate(blocks[0], newBlockPosition, Quaternion.identity);
                _blocks[(int)newBlockPosition.x * Level.Height * Level.Depth + (int)newBlockPosition.y * Level.Depth + (int)newBlockPosition.z].color = 1;
                _blocks[(int)newBlockPosition.x * Level.Height * Level.Depth + (int)newBlockPosition.y * Level.Depth + (int)newBlockPosition.z].isInvincible = false;
            }

            else if (hitInfo.collider.transform.position.z - hitInfo.point.z <= -BlockSize / 2)
            {
                var newBlockPosition = hitInfo.collider.transform.position + new Vector3(0, 0, BlockSize);
                Instantiate(blocks[0], newBlockPosition, Quaternion.identity);
                _blocks[(int)newBlockPosition.x * Level.Height * Level.Depth + (int)newBlockPosition.y * Level.Depth + (int)newBlockPosition.z].color = 1;
                _blocks[(int)newBlockPosition.x * Level.Height * Level.Depth + (int)newBlockPosition.y * Level.Depth + (int)newBlockPosition.z].isInvincible = false;
            }
        }

        if (Input.GetMouseButtonDown(0) && hitInfo.collider.gameObject.CompareTag("Block"))
        {
            var cube = hitInfo.collider.gameObject;
            Destroy(cube);
            _blocks[(int)cube.transform.position.x * Level.Height * Level.Depth + (int)cube.transform.position.y * Level.Depth + (int)cube.transform.position.z].color = 0;
            _blocks[(int)cube.transform.position.x * Level.Height * Level.Depth + (int)cube.transform.position.y * Level.Depth + (int)cube.transform.position.z].isInvincible = false;
        }

        if (Input.GetMouseButtonDown(0) && hitInfo.collider.gameObject.CompareTag("TNT"))
        {
            var cube = hitInfo.collider.gameObject;
            cube.GetComponent<Explosion>().Explode();
        }
    }

    private void OnDestroy()
    {
        LevelWriter.SaveLevel(RomanChe, _blocks);
    }

}