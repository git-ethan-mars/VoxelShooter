using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
namespace VoxelMap
{
    [BurstCompile]
    public struct RegenerateMeshJob : IJob
    {
        [ReadOnly]
        public NativeArray<VoxelData> Voxels;
        [ReadOnly]
        public NativeArray<Face> Faces;
        public NativeList<VertexData> Vertices;
        [WriteOnly]
        public NativeList<int> Triangles;

        public void Execute()
        {
            Vertices.Clear();
            Triangles.Clear();
            for (var i = 0; i < Voxels.Length; i++)
            {
                var x = i / ChunkData.ChunkSizeSquared;
                var y = (i - x * ChunkData.ChunkSizeSquared) / ChunkData.ChunkSize;
                var z = i - x * ChunkData.ChunkSizeSquared - y * ChunkData.ChunkSize;
                if (Faces[i] == Face.None)
                {
                    continue;
                }

                if (HasFlag(Faces[i], Face.Top))
                {
                    ChunkGeneratorHelper.GenerateTopSide(x, y, z, Voxels[i].Color, Vertices, Triangles);
                }

                if (HasFlag(Faces[i], Face.Bottom))
                {
                    ChunkGeneratorHelper.GenerateBottomSide(x, y, z, Voxels[i].Color, Vertices, Triangles);
                }

                if (HasFlag(Faces[i], Face.Front))
                {
                    ChunkGeneratorHelper.GenerateFrontSide(x, y, z, Voxels[i].Color, Vertices, Triangles);
                }

                if (HasFlag(Faces[i], Face.Back))
                {
                    ChunkGeneratorHelper.GenerateBackSide(x, y, z, Voxels[i].Color, Vertices, Triangles);
                }

                if (HasFlag(Faces[i], Face.Right))
                {
                    ChunkGeneratorHelper.GenerateRightSide(x, y, z, Voxels[i].Color, Vertices, Triangles);
                }

                if (HasFlag(Faces[i], Face.Left))
                {
                    ChunkGeneratorHelper.GenerateLeftSide(x, y, z, Voxels[i].Color, Vertices, Triangles);
                }
            }
        }

        private bool HasFlag(Face source, Face flag) => (byte)(source & flag) == (byte)flag;
    }
}