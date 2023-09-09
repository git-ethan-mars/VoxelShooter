using System;
using System.Linq;
using Data;
using Entities;
using Infrastructure.AssetManagement;
using MapLogic;
using Rendering;
using UnityEngine;
using Random = UnityEngine.Random;

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

        public void CreateFallingMesh(MeshData meshData,
            FallingMeshFallingMeshParticlePool fallingMeshFallingMeshParticlePool)
        {
            var mesh = new Mesh();
            mesh.SetVertices(meshData.Vertices.ToArray());
            mesh.SetTriangles(meshData.Triangles, 0);
            mesh.SetColors(meshData.Colors);
            mesh.SetNormals(meshData.Normals);
            mesh.indexFormat = meshData.IndexFormat;
            var fallingMesh = _assets.Instantiate(FallingMeshPath);
            fallingMesh.GetComponent<MeshFilter>().mesh = mesh;
            fallingMesh.GetComponent<FallingMesh>().Construct(fallingMeshFallingMeshParticlePool);
            var torque = new Vector3(Random.Range(0, 40), 0, Random.Range(0, 40));
            fallingMesh.GetComponent<Rigidbody>().AddTorque(torque);
            var meshCollider = fallingMesh.GetComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
        }

        public GameObject CreateChunkMeshRender(Vector3 position, Quaternion rotation, Transform parent)
        {
            return _assets.Instantiate(ChunkMeshRendererPath, position, rotation, parent);
        }

        public void CreateWalls(IMapProvider mapProvider, Transform parent)
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