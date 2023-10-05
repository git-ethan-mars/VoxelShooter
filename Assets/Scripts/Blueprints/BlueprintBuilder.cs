using System.Collections.Generic;
using Generators;
using Rendering;
using UnityEngine;

public class BlueprintBuilder
{
    private const int Size = 10;

    private readonly MeshFilter _meshFilter;

    private readonly MeshCollider _meshCollider;

    public Color32 DesiredColor { get; set; }

    private readonly List<Vector3> _vertices;

    private readonly List<int> _triangles;

    private readonly List<Color32> _colors;

    private readonly List<Vector3> _normals;

    private readonly Raycaster _raycaster;

    private Dictionary<Vector3Int, Color32> _blockByPositions;

    private Bounds _bounds;

    public BlueprintBuilder(MeshFilter meshFilter, MeshCollider meshCollider, Transform plane)
    {
        _meshFilter = meshFilter;
        _meshCollider = meshCollider;
        _raycaster = new Raycaster(Camera.main);
        plane.localScale = (float) Size / 10 * Vector3.one;
        _blockByPositions = new Dictionary<Vector3Int, Color32>();
        _vertices = new List<Vector3>();
        _triangles = new List<int>();
        _colors = new List<Color32>();
        _normals = new List<Vector3>();
        _bounds = new Bounds(new Vector3(0, (float) Size / 2, 0),
            new Vector3(Size, Size, Size));
        DesiredColor = new Color32(1, 1, 1, 1);
    }

    public void CreateNewBlueprint()
    {
        _blockByPositions.Clear();
        RedrawMesh();
    }

    public void CreateExistedBlueprint(Dictionary<Vector3Int, Color32> blueprintData)
    {
        _blockByPositions = blueprintData;
        RedrawMesh();
    }

    public void CreateBlock()
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
            RedrawMesh();
        }
    }

    public void RemoveBlock()
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
            RedrawMesh();
        }
    }

    private void RedrawMesh()
    {
        ClearMeshData();
        WriteMeshData();
        DrawMesh();
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
        _meshFilter.mesh = mesh;
        if (_vertices.Count == 0)
        {
            _meshCollider.sharedMesh = null;
        }
        else
        {
            _meshFilter.mesh = mesh;
            _meshCollider.sharedMesh = mesh;
        }
    }
}