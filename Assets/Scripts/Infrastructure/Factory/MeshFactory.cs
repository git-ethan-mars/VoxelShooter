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
        private const string PhysicMaterial = "Physics Materials/HighFrictionMaterial";
        private readonly IAssetProvider _assets;
        private const int MaxVerticesForMeshCollider = 24000;

        public MeshFactory(IAssetProvider assets)
        {
            _assets = assets;
        }

        public void CreateFallingMesh(MeshData meshData,
            FallingMeshParticlePool fallingMeshParticlePool)
        {
            var mesh = new Mesh();
            mesh.indexFormat = meshData.IndexFormat;
            mesh.SetVertices(meshData.Vertices.ToArray());
            mesh.SetTriangles(meshData.Triangles, 0);
            mesh.SetColors(meshData.Colors);
            mesh.SetNormals(meshData.Normals);
            var fallingMesh = _assets.Instantiate(FallingMeshPath);
            fallingMesh.GetComponent<MeshFilter>().mesh = mesh;
            fallingMesh.GetComponent<FallingMesh>().Construct(fallingMeshParticlePool);
            var torque = new Vector3(Random.Range(0, 40), 0, Random.Range(0, 40));
            fallingMesh.GetComponent<Rigidbody>().AddTorque(torque);
            if (meshData.Vertices.Count > MaxVerticesForMeshCollider)
            {
                fallingMesh.AddComponent<SphereCollider>();
                var sphereCollider = fallingMesh.GetComponent<SphereCollider>();
                var meshRenderer = fallingMesh.GetComponent<MeshRenderer>().bounds;
                sphereCollider.center = meshRenderer.center;
                sphereCollider.radius = 1;
                sphereCollider.material = _assets.Load<PhysicMaterial>(PhysicMaterial);
            }
            else
            {
                fallingMesh.AddComponent<MeshCollider>();
                var meshCollider = fallingMesh.GetComponent<MeshCollider>();
                meshCollider.convex = true;
                meshCollider.sharedMesh = mesh;
                meshCollider.material = _assets.Load<PhysicMaterial>(PhysicMaterial);
            }
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