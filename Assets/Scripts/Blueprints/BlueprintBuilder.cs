using System.Collections.Generic;
using Generators;
using Infrastructure.Services.Input;
using Rendering;
using UnityEngine;

public class BlueprintBuilder : MonoBehaviour
{
    private const int Size = 10;
    public Color32 DesiredColor { get; set; }

    [SerializeField]
    private MeshFilter meshFilter;

    [SerializeField]
    private MeshCollider meshCollider;

    [SerializeField]
    private Camera camera;

    [SerializeField]
    private Transform plane;

    private Dictionary<Vector3Int, Color32> _blockByPositions;
    private List<Vector3> _vertices;
    private List<int> _triangles;
    private List<Color32> _colors;
    private List<Vector3> _normals;

    private IInputService _inputService;
    private Raycaster _raycaster;

    private Bounds _bounds;

    private void Awake()
    {
        _inputService = new StandaloneInputService();
        _raycaster = new Raycaster(camera);
        plane.localScale = (float) Size / 10 * Vector3.one;
        _blockByPositions = new Dictionary<Vector3Int, Color32>();
        _vertices = new List<Vector3>();
        _triangles = new List<int>();
        _colors = new List<Color32>();
        _normals = new List<Vector3>();
        _bounds = new Bounds(new Vector3(0, (float) Size / 2, 0),
            new Vector3(Size, Size, Size));
        DesiredColor = new Color32(1, 1, 1, 1);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (_inputService.IsFirstActionButtonDown())
        {
            if (Physics.Raycast(_raycaster.CentredRay, out var hit))
            {
                var blockPosition = Vector3Int.FloorToInt(hit.point + hit.normal / 2) +
                                    new Vector3(0.5f, 0.5f, 0.5f);
                if (!_bounds.Contains(blockPosition))
                {
                    return;
                }

                _blockByPositions.TryAdd(Vector3Int.FloorToInt(blockPosition), DesiredColor);
                ClearMeshData();
                WriteMeshData();
                DrawMesh();
            }
        }

        if (_inputService.IsSecondActionButtonDown())
        {
            if (Physics.Raycast(_raycaster.CentredRay, out var hit))
            {
                var blockPosition = Vector3Int.FloorToInt(hit.point - hit.normal / 2) +
                                    new Vector3(0.5f, 0.5f, 0.5f);
                if (!_bounds.Contains(blockPosition))
                {
                    return;
                }

                _blockByPositions.Remove(Vector3Int.FloorToInt(blockPosition));
                ClearMeshData();
                WriteMeshData();
                DrawMesh();
            }
        }
    }

    private void WriteMeshData()
    {
        foreach (var (blockPosition, color) in _blockByPositions)
        {
            ChunkGeneratorHelper.GenerateTopSide(blockPosition.x, blockPosition.y, blockPosition.z, color,
                _vertices, _normals, _colors, _triangles);
            ChunkGeneratorHelper.GenerateBottomSide(blockPosition.x, blockPosition.y, blockPosition.z, color,
                _vertices, _normals, _colors, _triangles);
            ChunkGeneratorHelper.GenerateFrontSide(blockPosition.x, blockPosition.y, blockPosition.z, color,
                _vertices, _normals, _colors, _triangles);
            ChunkGeneratorHelper.GenerateBackSide(blockPosition.x, blockPosition.y, blockPosition.z, color,
                _vertices, _normals, _colors, _triangles);
            ChunkGeneratorHelper.GenerateRightSide(blockPosition.x, blockPosition.y, blockPosition.z, color,
                _vertices, _normals, _colors, _triangles);
            ChunkGeneratorHelper.GenerateLeftSide(blockPosition.x, blockPosition.y, blockPosition.z, color,
                _vertices, _normals, _colors, _triangles);
        }
    }

    private void ClearMeshData()
    {
        _vertices.Clear();
        _triangles.Clear();
        _normals.Clear();
        _colors.Clear();
    }

    private void DrawMesh()
    {
        var mesh = new Mesh();
        mesh.SetVertices(_vertices.ToArray());
        mesh.SetTriangles(_triangles.ToArray(), 0);
        mesh.SetColors(_colors);
        mesh.SetNormals(_normals);
        meshFilter.mesh = mesh;
        if (_vertices.Count == 0)
        {
            meshCollider.sharedMesh = null;
        }
        else
        {
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
        }
    }
}