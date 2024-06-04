using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Generators
{
    public class ChunkMesh
    {
        public ChunkMesh UpperNeighbour { get; set; }
        public ChunkMesh LowerNeighbour { get; set; }
        public ChunkMesh LeftNeighbour { get; set; }
        public ChunkMesh RightNeighbour { get; set; }
        public ChunkMesh FrontNeighbour { get; set; }
        public ChunkMesh BackNeighbour { get; set; }
        private readonly Mesh _mesh;
        private readonly MeshData _meshData;
        private readonly ChunkData _chunkData;
        private readonly GameObject _chunkMeshObject;

        public ChunkMesh(GameObject chunkMeshObject, ChunkData chunkData, MeshData meshData)
        {
            _chunkData = chunkData;
            _meshData = meshData;
            _chunkMeshObject = chunkMeshObject;
            _mesh = new Mesh();
            ApplyMesh();
        }

        public void SpawnBlocks(List<BlockDataWithPosition> blocks)
        {
            var neighbourChunksToRegenerate = Faces.None;
            for (var i = 0; i < blocks.Count; i++)
            {
                var x = blocks[i].Position.x;
                var y = blocks[i].Position.y;
                var z = blocks[i].Position.z;
                _chunkData.SetBlock(x, y, z, blocks[i].BlockData);
                SetFaces(x, y, z);
                SetNeighboursFaces(blocks[i].Position);
                if (z == ChunkData.ChunkSize - 1 && FrontNeighbour is not null)
                {
                    FrontNeighbour.SetFaces(x, y, 0);
                    neighbourChunksToRegenerate |= Faces.Front;
                }

                if (z == 0 && BackNeighbour is not null)
                {
                    BackNeighbour.SetFaces(x, y, ChunkData.ChunkSize - 1);
                    neighbourChunksToRegenerate |= Faces.Back;
                }

                if (y == ChunkData.ChunkSize - 1)
                {
                    if (UpperNeighbour is not null)
                    {
                        UpperNeighbour.SetFaces(x, 0, z);
                    }

                    neighbourChunksToRegenerate |= Faces.Top;
                }

                if (y == 0 && LowerNeighbour is not null)
                {
                    LowerNeighbour.SetFaces(x, ChunkData.ChunkSize - 1, z);
                    neighbourChunksToRegenerate |= Faces.Bottom;
                }

                if (x == ChunkData.ChunkSize - 1 && RightNeighbour is not null)
                {
                    RightNeighbour.SetFaces(0, y, z);
                    neighbourChunksToRegenerate |= Faces.Right;
                }
                else if (x == 0 && LeftNeighbour is not null)
                {
                    LeftNeighbour.SetFaces(ChunkData.ChunkSize - 1, y, z);
                    neighbourChunksToRegenerate |= Faces.Left;
                }
            }

            RegenerateMesh();
            if (neighbourChunksToRegenerate.HasFlag(Faces.Front))
            {
                FrontNeighbour!.RegenerateMesh();
            }

            if (neighbourChunksToRegenerate.HasFlag(Faces.Back))
            {
                BackNeighbour!.RegenerateMesh();
            }

            if (neighbourChunksToRegenerate.HasFlag(Faces.Top) && UpperNeighbour is not null)
            {
                UpperNeighbour.RegenerateMesh();
            }

            if (neighbourChunksToRegenerate.HasFlag(Faces.Bottom))
            {
                LowerNeighbour!.RegenerateMesh();
            }

            if (neighbourChunksToRegenerate.HasFlag(Faces.Right))
            {
                RightNeighbour!.RegenerateMesh();
            }

            if (neighbourChunksToRegenerate.HasFlag(Faces.Left))
            {
                LeftNeighbour!.RegenerateMesh();
            }
        }

        private void ApplyMesh()
        {
            _mesh.SetVertices(_meshData.Vertices);
            _mesh.SetTriangles(_meshData.Triangles, 0);
            _mesh.SetColors(_meshData.Colors);
            _mesh.SetNormals(_meshData.Normals);
            if (_meshData.Vertices.Count == 0)
            {
                _chunkMeshObject.GetComponent<MeshCollider>().sharedMesh = null;
            }
            else
            {
                _chunkMeshObject.GetComponent<MeshFilter>().mesh = _mesh;
                _chunkMeshObject.GetComponent<MeshCollider>().sharedMesh = _mesh;
            }
        }

        private void SetNeighboursFaces(Vector3Int blockPosition)
        {
            if (ChunkData.IsValidPosition(blockPosition.x + 1, blockPosition.y, blockPosition.z))
            {
                SetFaces(blockPosition.x + 1, blockPosition.y, blockPosition.z);
            }

            if (ChunkData.IsValidPosition(blockPosition.x - 1, blockPosition.y, blockPosition.z))
            {
                SetFaces(blockPosition.x - 1, blockPosition.y, blockPosition.z);
            }

            if (ChunkData.IsValidPosition(blockPosition.x, blockPosition.y + 1, blockPosition.z))
            {
                SetFaces(blockPosition.x, blockPosition.y + 1, blockPosition.z);
            }

            if (ChunkData.IsValidPosition(blockPosition.x, blockPosition.y - 1, blockPosition.z))
            {
                SetFaces(blockPosition.x, blockPosition.y - 1, blockPosition.z);
            }

            if (ChunkData.IsValidPosition(blockPosition.x, blockPosition.y, blockPosition.z + 1))
            {
                SetFaces(blockPosition.x, blockPosition.y, blockPosition.z + 1);
            }

            if (ChunkData.IsValidPosition(blockPosition.x, blockPosition.y, blockPosition.z - 1))
            {
                SetFaces(blockPosition.x, blockPosition.y, blockPosition.z - 1);
            }
        }

        private void RegenerateMesh()
        {
            _mesh.Clear();
            _meshData.Vertices.Clear();
            _meshData.Triangles.Clear();
            _meshData.Colors.Clear();
            _mesh.Clear();
            _meshData.Normals.Clear();
            for (var x = 0; x < ChunkData.ChunkSize; x++)
            {
                for (var y = 0; y < ChunkData.ChunkSize; y++)
                {
                    for (var z = 0; z < ChunkData.ChunkSize; z++)
                    {
                        if (_chunkData.GetFaces(x, y, z) == Faces.None)
                        {
                            continue;
                        }

                        var color = _chunkData.GetBlock(x, y, z).Color;
                        if (_chunkData.GetFaces(x, y, z).HasFlag(Faces.Top))
                        {
                            ChunkGeneratorHelper.GenerateTopSide(x, y, z, color, _meshData.Vertices, _meshData.Normals,
                                _meshData.Colors, _meshData.Triangles);
                        }

                        if (_chunkData.GetFaces(x, y, z).HasFlag(Faces.Bottom))
                        {
                            ChunkGeneratorHelper.GenerateBottomSide(x, y, z, color, _meshData.Vertices,
                                _meshData.Normals,
                                _meshData.Colors, _meshData.Triangles);
                        }

                        if (_chunkData.GetFaces(x, y, z).HasFlag(Faces.Front))
                        {
                            ChunkGeneratorHelper.GenerateFrontSide(x, y, z, color, _meshData.Vertices,
                                _meshData.Normals,
                                _meshData.Colors, _meshData.Triangles);
                        }

                        if (_chunkData.GetFaces(x, y, z).HasFlag(Faces.Back))
                        {
                            ChunkGeneratorHelper.GenerateBackSide(x, y, z, color, _meshData.Vertices, _meshData.Normals,
                                _meshData.Colors, _meshData.Triangles);
                        }

                        if (_chunkData.GetFaces(x, y, z).HasFlag(Faces.Left))
                        {
                            ChunkGeneratorHelper.GenerateLeftSide(x, y, z, color, _meshData.Vertices, _meshData.Normals,
                                _meshData.Colors, _meshData.Triangles);
                        }

                        if (_chunkData.GetFaces(x, y, z).HasFlag(Faces.Right))
                        {
                            ChunkGeneratorHelper.GenerateRightSide(x, y, z, color, _meshData.Vertices,
                                _meshData.Normals,
                                _meshData.Colors, _meshData.Triangles);
                        }
                    }
                }
            }

            ApplyMesh();
        }

        private void SetFaces(int x, int y, int z)
        {
            var resultFaces = Faces.None;
            if (!_chunkData.GetBlock(x, y, z).IsSolid())
            {
                _chunkData.SetFaces(x, y, z, resultFaces);
                return;
            }

            if (!ChunkData.IsValidPosition(x, y + 1, z) && (UpperNeighbour is null || UpperNeighbour is not null &&
                    !UpperNeighbour._chunkData.GetBlock(x, 0, z).IsSolid()) ||
                ChunkData.IsValidPosition(x, y + 1, z) && !_chunkData.GetBlock(x, y + 1, z).IsSolid())
            {
                resultFaces |= Faces.Top;
            }

            if (!ChunkData.IsValidPosition(x, y - 1, z) && LowerNeighbour is not null &&
                !LowerNeighbour._chunkData.GetBlock(x, ChunkData.ChunkSize - 1, z).IsSolid() ||
                ChunkData.IsValidPosition(x, y - 1, z) && !_chunkData.GetBlock(x, y - 1, z).IsSolid())
            {
                resultFaces |= Faces.Bottom;
            }

            if (!ChunkData.IsValidPosition(x, y, z + 1) && FrontNeighbour is not null &&
                !FrontNeighbour._chunkData.GetBlock(x, y, 0).IsSolid() ||
                ChunkData.IsValidPosition(x, y, z + 1) && !_chunkData.GetBlock(x, y, z + 1).IsSolid())
            {
                resultFaces |= Faces.Front;
            }

            if (!ChunkData.IsValidPosition(x, y, z - 1) && BackNeighbour is not null &&
                !BackNeighbour._chunkData.GetBlock(x, y, ChunkData.ChunkSize - 1).IsSolid() ||
                ChunkData.IsValidPosition(x, y, z - 1) && !_chunkData.GetBlock(x, y, z - 1).IsSolid())
            {
                resultFaces |= Faces.Back;
            }

            if (!ChunkData.IsValidPosition(x + 1, y, z) && RightNeighbour is not null &&
                !RightNeighbour._chunkData.GetBlock(0, y, z).IsSolid() ||
                ChunkData.IsValidPosition(x + 1, y, z) && !_chunkData.GetBlock(x + 1, y, z).IsSolid())
            {
                resultFaces |= Faces.Right;
            }

            if (!ChunkData.IsValidPosition(x - 1, y, z) && LeftNeighbour is not null &&
                !LeftNeighbour._chunkData.GetBlock(ChunkData.ChunkSize - 1, y, z).IsSolid() ||
                ChunkData.IsValidPosition(x - 1, y, z) && !_chunkData.GetBlock(x - 1, y, z).IsSolid())
            {
                resultFaces |= Faces.Left;
            }

            _chunkData.SetFaces(x, y, z, resultFaces);
        }
    }
}