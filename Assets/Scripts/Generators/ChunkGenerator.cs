using System.Collections.Generic;
using Data;
using Infrastructure.Factory;
using Unity.Collections;
using UnityEngine;

namespace Generators
{
    public class ChunkMeshGenerator
    {
        private readonly GameObject _chunkMeshRenderer;
        public ChunkMeshGenerator UpperNeighbour { get; set; }
        public ChunkMeshGenerator LowerNeighbour { get; set; }
        public ChunkMeshGenerator LeftNeighbour { get; set; }
        public ChunkMeshGenerator RightNeighbour { get; set; }
        public ChunkMeshGenerator FrontNeighbour { get; set; }
        public ChunkMeshGenerator BackNeighbour { get; set; }
        private Mesh ChunkMesh { get; set; }
        public ChunkData ChunkData { get; set; }
        public NativeList<Vector3> Vertices { get; private set; }
        public NativeList<int> Triangles { get; private set; }
        public NativeList<Color32> Colors { get; private set; }
        public NativeList<Vector3> Normals { get; private set; }

        public ChunkMeshGenerator(GameObject chunkMeshRenderer)
        {
            ChunkMesh = new Mesh();
            Vertices = new NativeList<Vector3>(Allocator.Persistent);
            Triangles = new NativeList<int>(Allocator.Persistent);
            Colors = new NativeList<Color32>(Allocator.Persistent);
            Normals = new NativeList<Vector3>(Allocator.Persistent);
            _chunkMeshRenderer = chunkMeshRenderer;
        }

        public void ApplyMesh()
        {
            ChunkMesh.SetVertices(Vertices.AsArray());
            var triangles = new int[Triangles.Length];
            for (var i = 0; i < Triangles.Length; i++)
            {
                triangles[i] = Triangles[i];
            }

            ChunkMesh.SetTriangles(triangles, 0);
            ChunkMesh.SetColors(Colors.AsArray());
            ChunkMesh.SetNormals(Normals.AsArray());
            if (Vertices.Length == 0)
            {
                _chunkMeshRenderer.GetComponent<MeshCollider>().sharedMesh = null;
            }
            else
            {
                _chunkMeshRenderer.GetComponent<MeshFilter>().mesh = ChunkMesh;
                _chunkMeshRenderer.GetComponent<MeshCollider>().sharedMesh = ChunkMesh;
            }
        }
        
        public void SpawnBlocks(List<(Vector3Int, BlockData)> data)
        {
            var neighbourChunksToRegenerate = Faces.None;
            for (var i = 0; i < data.Count; i++)
            {
                var blockPosition = data[i].Item1;
                var blockData = data[i].Item2;
                ChunkData.Blocks[
                    blockPosition.x * ChunkData.ChunkSizeSquared + blockPosition.y * ChunkData.ChunkSize +
                    blockPosition.z] = blockData;
                SetFaces(blockPosition.x, blockPosition.y, blockPosition.z);
                SetNeighboursFaces(blockPosition);
                if (blockPosition.z == ChunkData.ChunkSize - 1 && FrontNeighbour is not null)
                {
                    FrontNeighbour.SetFaces(blockPosition.x, blockPosition.y, 0);
                    neighbourChunksToRegenerate |= Faces.Front;
                }

                if (blockPosition.z == 0 && BackNeighbour is not null)
                {
                    BackNeighbour.SetFaces(blockPosition.x, blockPosition.y, ChunkData.ChunkSize - 1);
                    neighbourChunksToRegenerate |= Faces.Back;
                }

                if (blockPosition.y == ChunkData.ChunkSize - 1)
                {
                    if (UpperNeighbour is not null)
                        UpperNeighbour.SetFaces(blockPosition.x, 0, blockPosition.z);
                    neighbourChunksToRegenerate |= Faces.Top;
                }

                if (blockPosition.y == 0 && LowerNeighbour is not null)
                {
                    LowerNeighbour.SetFaces(blockPosition.x, ChunkData.ChunkSize - 1, blockPosition.z);
                    neighbourChunksToRegenerate |= Faces.Bottom;
                }

                if (blockPosition.x == ChunkData.ChunkSize - 1 && RightNeighbour is not null)
                {
                    RightNeighbour.SetFaces(0, blockPosition.y, blockPosition.z);
                    neighbourChunksToRegenerate |= Faces.Right;
                }
                else if (blockPosition.x == 0 && LeftNeighbour is not null)
                {
                    LeftNeighbour.SetFaces(ChunkData.ChunkSize - 1, blockPosition.y, blockPosition.z);
                    neighbourChunksToRegenerate |= Faces.Left;
                }
            }

            RegenerateMesh();
            if (neighbourChunksToRegenerate.HasFlag(Faces.Front))
            {
                FrontNeighbour.RegenerateMesh();
            }

            if (neighbourChunksToRegenerate.HasFlag(Faces.Back))
            {
                BackNeighbour.RegenerateMesh();
            }

            if (neighbourChunksToRegenerate.HasFlag(Faces.Top) && UpperNeighbour is not null)
            {
                UpperNeighbour.RegenerateMesh();
            }

            if (neighbourChunksToRegenerate.HasFlag(Faces.Bottom))
            {
                LowerNeighbour.RegenerateMesh();
            }

            if (neighbourChunksToRegenerate.HasFlag(Faces.Right))
            {
                RightNeighbour.RegenerateMesh();
            }

            if (neighbourChunksToRegenerate.HasFlag(Faces.Left))
            {
                LeftNeighbour.RegenerateMesh();
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
            Vertices.Clear();
            Triangles.Clear();
            Colors.Clear();
            ChunkMesh.Clear();
            Normals.Clear();
            for (var i = 0; i < ChunkData.ChunkSizeCubed; i++)
            {
                var x = i / ChunkData.ChunkSizeSquared;
                var y = (i - x * ChunkData.ChunkSizeSquared) / ChunkData.ChunkSize;
                var z = i - x * ChunkData.ChunkSizeSquared - y * ChunkData.ChunkSize;
                if (ChunkData.Faces[i] == Faces.None) continue;
                var color = ChunkData.Blocks[i].Color;
                if (ChunkData.Faces[i].HasFlag(Faces.Top))
                {
                    ChunkGeneratorHelper.GenerateTopSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }

                if (ChunkData.Faces[i].HasFlag(Faces.Bottom))
                {
                    ChunkGeneratorHelper.GenerateBottomSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }

                if (ChunkData.Faces[i].HasFlag(Faces.Front))
                {
                    ChunkGeneratorHelper.GenerateFrontSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }

                if (ChunkData.Faces[i].HasFlag(Faces.Back))
                {
                    ChunkGeneratorHelper.GenerateBackSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }

                if (ChunkData.Faces[i].HasFlag(Faces.Left))
                {
                    ChunkGeneratorHelper.GenerateLeftSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }

                if (ChunkData.Faces[i].HasFlag(Faces.Right))
                {
                    ChunkGeneratorHelper.GenerateRightSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }
            }

            ApplyMesh();
        }

        private void SetFaces(int x, int y, int z)
        {
            ChunkData.Faces[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z] = Faces.None;
            if (ChunkData.Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].Color
                .Equals(BlockColor.empty)) return;
            if (!ChunkData.IsValidPosition(x, y + 1, z) && (UpperNeighbour is null || UpperNeighbour is not null &&
                    UpperNeighbour.ChunkData.Blocks[x * ChunkData.ChunkSizeSquared + z].Color
                        .Equals(BlockColor.empty)) || ChunkData.IsValidPosition(x, y + 1, z) && ChunkData
                    .Blocks[x * ChunkData.ChunkSizeSquared + (y + 1) * ChunkData.ChunkSize + z].Color
                    .Equals(BlockColor.empty))
            {
                ChunkData.Faces[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z] |= Faces.Top;
            }

            if (!ChunkData.IsValidPosition(x, y - 1, z) && LowerNeighbour is not null && LowerNeighbour.ChunkData
                    .Blocks[x * ChunkData.ChunkSizeSquared + (ChunkData.ChunkSize - 1) * ChunkData.ChunkSize + z].Color
                    .Equals(BlockColor.empty) || ChunkData.IsValidPosition(x, y - 1, z) && ChunkData
                    .Blocks[x * ChunkData.ChunkSizeSquared + (y - 1) * ChunkData.ChunkSize + z].Color
                    .Equals(BlockColor.empty))
            {
                ChunkData.Faces[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z] |= Faces.Bottom;
            }

            if (!ChunkData.IsValidPosition(x, y, z + 1) && FrontNeighbour is not null &&
                FrontNeighbour.ChunkData.Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize].Color
                    .Equals(BlockColor.empty) || ChunkData.IsValidPosition(x, y, z + 1) && ChunkData
                    .Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z + 1].Color
                    .Equals(BlockColor.empty))
            {
                ChunkData.Faces[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z] |= Faces.Front;
            }

            if (!ChunkData.IsValidPosition(x, y, z - 1) && BackNeighbour is not null &&
                BackNeighbour.ChunkData
                    .Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + ChunkData.ChunkSize - 1].Color
                    .Equals(BlockColor.empty) || ChunkData.IsValidPosition(x, y, z - 1) && ChunkData
                    .Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z - 1].Color
                    .Equals(BlockColor.empty))
            {
                ChunkData.Faces[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z] |= Faces.Back;
            }

            if (!ChunkData.IsValidPosition(x + 1, y, z) && RightNeighbour is not null &&
                RightNeighbour.ChunkData.Blocks[y * ChunkData.ChunkSize + z].Color.Equals(BlockColor.empty) ||
                ChunkData.IsValidPosition(x + 1, y, z) && ChunkData
                    .Blocks[(x + 1) * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].Color
                    .Equals(BlockColor.empty))
            {
                ChunkData.Faces[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z] |= Faces.Right;
            }

            if (!ChunkData.IsValidPosition(x - 1, y, z) && LeftNeighbour is not null && LeftNeighbour.ChunkData
                    .Blocks[(ChunkData.ChunkSize - 1) * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].Color
                    .Equals(BlockColor.empty) || ChunkData.IsValidPosition(x - 1, y, z) && ChunkData
                    .Blocks[(x - 1) * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].Color
                    .Equals(BlockColor.empty))
            {
                ChunkData.Faces[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z] |= Faces.Left;
            }
        }

        private void OnDestroy()
        {
            Vertices.Dispose();
            Normals.Dispose();
            Colors.Dispose();
            Triangles.Dispose();
            ChunkData.Blocks.Dispose();
            ChunkData.Faces.Dispose();
        }
    }
}