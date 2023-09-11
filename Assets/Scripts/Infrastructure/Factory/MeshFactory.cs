using System;
using System.Linq;
using Data;
using Infrastructure.AssetManagement;
using MapLogic;
using Rendering;
using UnityEngine;

namespace Infrastructure.Factory
{
    public class MeshFactory : IMeshFactory
    {
        private const string FallingMeshPath = "Prefabs/MapCreation/FallingMesh";
        private const string ChunkMeshRendererPath = "Prefabs/MapCreation/Chunk";
        private const string WallPath = "Prefabs/MapCreation/Wall";
        private readonly IAssetProvider _assets;

        public MeshFactory(IAssetProvider assets)
        {
            _assets = assets;
        }

        public void CreateFallingMesh(MeshData meshData)
        {
            var mesh = new Mesh();
            mesh.SetVertices(meshData.Vertices.ToArray());
            mesh.SetTriangles(meshData.Triangles, 0);
            mesh.SetColors(meshData.Colors);
            mesh.SetNormals(meshData.Normals);
            mesh.indexFormat = meshData.IndexFormat;
            var fallingMesh = _assets.Instantiate(FallingMeshPath);
            fallingMesh.GetComponent<MeshFilter>().mesh = mesh;
            var meshCollider = fallingMesh.GetComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
        }

        public GameObject CreateChunkMeshRender(Vector3 position, Quaternion rotation, Transform parent)
        {
            return _assets.Instantiate(ChunkMeshRendererPath, position, rotation, parent);
        }

        public void CreateWalls(MapProvider mapProvider, Transform parent)
        {
            var allFaces = Enum.GetValues(typeof(Faces)).Cast<Faces>().Where(face => face != Faces.None);
            foreach (var face in allFaces)
            {
                _assets.Instantiate(WallPath, parent).GetComponent<WallRenderer>().Construct(mapProvider, face);
            }
        }

        public GameObject CreateGameModel(GameObject prefab, Transform itemPosition)
        {
            return _assets.Instantiate(prefab, itemPosition);
        }
    }
}