using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
namespace VoxelMap
{
    [BurstCompile]
    public struct MeshPreparationJob : IJob
    {
        [WriteOnly]
        public Mesh.MeshData MeshData;

        [ReadOnly]
        public NativeList<VertexData> Vertices;

        [ReadOnly]
        public NativeList<int> Triangles;

        public void Execute()
        {
            var attributes = new NativeArray<VertexAttributeDescriptor>(3, Allocator.Temp);
            attributes[0] = new VertexAttributeDescriptor(VertexAttribute.Position);
            attributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal);
            attributes[2] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, dimension: 4);
            MeshData.SetVertexBufferParams(Vertices.Length, attributes);
            var meshVertices = MeshData.GetVertexData<VertexData>();
            meshVertices.CopyFrom(Vertices.AsArray());
            MeshData.SetIndexBufferParams(Triangles.Length, IndexFormat.UInt32);
            var meshTriangles = MeshData.GetIndexData<int>();
            meshTriangles.CopyFrom(Triangles.AsArray());
            MeshData.subMeshCount = 1;
            MeshData.SetSubMesh(0, new SubMeshDescriptor(0, Triangles.Length), NoCalculations);
        }

        public MeshUpdateFlags NoCalculations => MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags
            .DontNotifyMeshUsers | MeshUpdateFlags.DontResetBoneBounds;
    }
}