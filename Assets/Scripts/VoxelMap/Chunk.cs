using System;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine;

namespace VoxelMap
{
    public class Chunk : MonoBehaviour, IDisposable
    {
        [SerializeField]
        private MeshFilter meshFilter;
        [SerializeField]
        private MeshCollider meshCollider;

        public Dictionary<ChunkNeighbourType, Chunk> Neighbours { get; set; } = new();
        private Mesh _mesh;
        private ChunkData _chunkData;
        private NativeList<VertexData> _vertices;
        private NativeList<int> _triangles;
        private NativeArray<Face> _faces;
        private bool _isDisposed;

        public void Construct(ChunkData chunkData, NativeArray<Face> faces)
        {
            _chunkData = chunkData;
            _vertices = new NativeList<VertexData>(Allocator.Persistent);
            _triangles = new NativeList<int>(Allocator.Persistent);
            _faces = faces;
            _mesh = new Mesh();
            _mesh.bounds = new Bounds(Vector3.one * ChunkData.ChunkSize / 2, Vector3.one * ChunkData.ChunkSize);
        }

        public void ChangeVoxels(List<Voxel> voxels)
        {
            var neighbourChunksToRegenerate = Face.None;
            for (var i = 0; i < voxels.Count; i++)
            {
                var x = voxels[i].Position.x;
                var y = voxels[i].Position.y;
                var z = voxels[i].Position.z;
                _chunkData.SetVoxel(x, y, z, voxels[i].Data, PositionType.Local);
                UpdateVoxelFaces(x, y, z);
                SetNeighboursFaces(voxels[i].Position);
                if (z == ChunkData.ChunkSize - 1 && Neighbours.TryGetValue(ChunkNeighbourType.Front, out var frontNeighbour))
                {
                    frontNeighbour.UpdateVoxelFaces(x, y, 0);
                    neighbourChunksToRegenerate |= Face.Front;
                }

                if (z == 0 && Neighbours.TryGetValue(ChunkNeighbourType.Back, out var backNeighbour))
                {
                    backNeighbour.UpdateVoxelFaces(x, y, ChunkData.ChunkSize - 1);
                    neighbourChunksToRegenerate |= Face.Back;
                }

                if (y == ChunkData.ChunkSize - 1)
                {
                    if (Neighbours.TryGetValue(ChunkNeighbourType.Up, out var upperNeighbour))
                    {
                        upperNeighbour.UpdateVoxelFaces(x, 0, z);
                    }

                    neighbourChunksToRegenerate |= Face.Top;
                }

                if (y == 0 && Neighbours.TryGetValue(ChunkNeighbourType.Down, out var lowerNeighbour))
                {
                    lowerNeighbour.UpdateVoxelFaces(x, ChunkData.ChunkSize - 1, z);
                    neighbourChunksToRegenerate |= Face.Bottom;
                }

                if (x == ChunkData.ChunkSize - 1 && Neighbours.TryGetValue(ChunkNeighbourType.Right, out var rightNeighbour))
                {
                    rightNeighbour.UpdateVoxelFaces(0, y, z);
                    neighbourChunksToRegenerate |= Face.Right;
                }
                else if (x == 0 && Neighbours.TryGetValue(ChunkNeighbourType.Left, out var leftNeighbour))
                {
                    leftNeighbour.UpdateVoxelFaces(ChunkData.ChunkSize - 1, y, z);
                    neighbourChunksToRegenerate |= Face.Left;
                }
            }

            RegenerateMesh();

            foreach (ChunkNeighbourType chunkNeighbour in Enum.GetValues(typeof(ChunkNeighbourType)))
            {
                if (neighbourChunksToRegenerate.HasFlag(chunkNeighbour))
                {
                    Neighbours[chunkNeighbour].RegenerateMesh();
                }
            }
        }

        private void SetNeighboursFaces(Vector3Int voxelPosition)
        {
            if (ChunkData.IsValidPosition(voxelPosition.x + 1, voxelPosition.y, voxelPosition.z))
            {
                UpdateVoxelFaces(voxelPosition.x + 1, voxelPosition.y, voxelPosition.z);
            }

            if (ChunkData.IsValidPosition(voxelPosition.x - 1, voxelPosition.y, voxelPosition.z))
            {
                UpdateVoxelFaces(voxelPosition.x - 1, voxelPosition.y, voxelPosition.z);
            }

            if (ChunkData.IsValidPosition(voxelPosition.x, voxelPosition.y + 1, voxelPosition.z))
            {
                UpdateVoxelFaces(voxelPosition.x, voxelPosition.y + 1, voxelPosition.z);
            }

            if (ChunkData.IsValidPosition(voxelPosition.x, voxelPosition.y - 1, voxelPosition.z))
            {
                UpdateVoxelFaces(voxelPosition.x, voxelPosition.y - 1, voxelPosition.z);
            }

            if (ChunkData.IsValidPosition(voxelPosition.x, voxelPosition.y, voxelPosition.z + 1))
            {
                UpdateVoxelFaces(voxelPosition.x, voxelPosition.y, voxelPosition.z + 1);
            }

            if (ChunkData.IsValidPosition(voxelPosition.x, voxelPosition.y, voxelPosition.z - 1))
            {
                UpdateVoxelFaces(voxelPosition.x, voxelPosition.y, voxelPosition.z - 1);
            }
        }

        public void RegenerateMesh(JobHandle dependOn = default)
        {
            var regenerateMeshJob = new RegenerateMeshJob
            {
                Voxels = _chunkData.Voxels,
                Vertices = _vertices,
                Triangles = _triangles,
                Faces = _faces
            };

            var regenerateMeshHandle = regenerateMeshJob.Schedule(dependOn);
            var meshArray = Mesh.AllocateWritableMeshData(1);
            var mainMesh = meshArray[0];
            var meshPreparationJob = new MeshPreparationJob()
            {
                MeshData = mainMesh, Vertices = _vertices, Triangles = _triangles
            }; 
            meshPreparationJob.Schedule(regenerateMeshHandle).Complete();
            Mesh.ApplyAndDisposeWritableMeshData(meshArray, _mesh, meshPreparationJob.NoCalculations);
            if (_vertices.Length == 0)
            {
                meshCollider.sharedMesh = null;
            }
            else
            {
                meshFilter.mesh = _mesh;
                meshCollider.sharedMesh = _mesh;
            }
        }

        private void UpdateVoxelFaces(int x, int y, int z)
        {
            var resultFaces = Face.None;
            if (!_chunkData.GetVoxel(x, y, z, PositionType.Local).IsSolid())
            {
                SetFaces(x, y, z, resultFaces);
                return;
            }

            var containsUpperChunk = Neighbours.TryGetValue(ChunkNeighbourType.Up, out var upperNeighbour);
            if (!ChunkData.IsValidPosition(x, y + 1, z) &&
                (!containsUpperChunk || !upperNeighbour._chunkData.GetVoxel(x, 0, z, PositionType.Local).IsSolid()) ||
                ChunkData.IsValidPosition(x, y + 1, z) && !_chunkData.GetVoxel(x, y + 1, z, PositionType.Local).IsSolid())
            {
                resultFaces |= Face.Top;
            }

            var containsLowerChunk = Neighbours.TryGetValue(ChunkNeighbourType.Down, out var lowerNeighbour);
            if (!ChunkData.IsValidPosition(x, y - 1, z) && containsLowerChunk &&
                !lowerNeighbour._chunkData.GetVoxel(x, ChunkData.ChunkSize - 1, z, PositionType.Local).IsSolid() ||
                ChunkData.IsValidPosition(x, y - 1, z) && !_chunkData.GetVoxel(x, y - 1, z, PositionType.Local).IsSolid())
            {
                resultFaces |= Face.Bottom;
            }

            var containsFrontChunk = Neighbours.TryGetValue(ChunkNeighbourType.Front, out var frontNeighbour);
            if (!ChunkData.IsValidPosition(x, y, z + 1) && containsFrontChunk &&
                !frontNeighbour._chunkData.GetVoxel(x, y, 0, PositionType.Local).IsSolid() ||
                ChunkData.IsValidPosition(x, y, z + 1) && !_chunkData.GetVoxel(x, y, z + 1, PositionType.Local).IsSolid())
            {
                resultFaces |= Face.Front;
            }

            var containsBackChunk = Neighbours.TryGetValue(ChunkNeighbourType.Back, out var backNeighbour);
            if (!ChunkData.IsValidPosition(x, y, z - 1) && containsBackChunk &&
                !backNeighbour._chunkData.GetVoxel(x, y, ChunkData.ChunkSize - 1, PositionType.Local).IsSolid() ||
                ChunkData.IsValidPosition(x, y, z - 1) && !_chunkData.GetVoxel(x, y, z - 1, PositionType.Local).IsSolid())
            {
                resultFaces |= Face.Back;
            }

            var containsRightChunk = Neighbours.TryGetValue(ChunkNeighbourType.Right, out var rightNeighbour);
            if (!ChunkData.IsValidPosition(x + 1, y, z) && containsRightChunk &&
                !rightNeighbour._chunkData.GetVoxel(0, y, z, PositionType.Local).IsSolid() ||
                ChunkData.IsValidPosition(x + 1, y, z) && !_chunkData.GetVoxel(x + 1, y, z, PositionType.Local).IsSolid())
            {
                resultFaces |= Face.Right;
            }

            var containsLeftChunk = Neighbours.TryGetValue(ChunkNeighbourType.Left, out var leftNeighbour);
            if (!ChunkData.IsValidPosition(x - 1, y, z) && containsLeftChunk &&
                !leftNeighbour._chunkData.GetVoxel(ChunkData.ChunkSize - 1, y, z, PositionType.Local).IsSolid() ||
                ChunkData.IsValidPosition(x - 1, y, z) && !_chunkData.GetVoxel(x - 1, y, z, PositionType.Local).IsSolid())
            {
                resultFaces |= Face.Left;
            }

            SetFaces(x, y, z, resultFaces);
        }

        private void SetFaces(int x, int y, int z, Face faces)
        {
            _faces[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z] = faces;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _vertices.Dispose();
            _triangles.Dispose();
            _faces.Dispose();
            _chunkData.Dispose();
            _isDisposed = true;
        }
    }
}