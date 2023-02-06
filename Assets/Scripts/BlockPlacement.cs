using UnityEngine;

// ReSharper disable PossibleLossOfFraction

public class BlockPlacement : MonoBehaviour
{
    [SerializeField] private List<GameObject> blocks;
    [SerializeField] private GameObject slicedBlock;
    [SerializeField] private float placeDistance;
    [SerializeField] private float explosionForce;
    private Camera _camera;

    private void Start()
    {
        CenterPosition = new Vector2(Screen.width / 2, Screen.height / 2);
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(0)) return;
        var ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        var raycastResult = Physics.Raycast(ray, out var hitInfo, placeDistance);
        if (!raycastResult) return;
        if (Input.GetMouseButtonDown(1) &&
            (hitInfo.collider.CompareTag("Chunk") || hitInfo.collider.gameObject.CompareTag("Floor")))
        {
            var newBlock = new Block {Kind = BlockKind.CommonBlock};
            GlobalEvents.OnBlockChangeState(newBlock, Vector3Int.FloorToInt(hitInfo.point + hitInfo.normal/2));
        }

        if (Input.GetMouseButtonDown(0) && hitInfo.collider.gameObject.CompareTag("Chunk"))
        {
            var destroyedBlock = new Block() {Kind = BlockKind.Empty};
            GlobalEvents.OnBlockChangeState(destroyedBlock, Vector3Int.FloorToInt(hitInfo.point + hitInfo.normal/2));
        }

        if (Input.GetMouseButtonDown(0) && hitInfo.collider.gameObject.CompareTag("TNT"))
        {
            var cube = hitInfo.collider.gameObject;
            cube.GetComponent<Explosion>().Explode();
        }
    }
}