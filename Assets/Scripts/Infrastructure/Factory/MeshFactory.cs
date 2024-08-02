using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Entities;
using Infrastructure.AssetManagement;
using MapLogic;
using Networking.ClientServices;
using Rendering;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Infrastructure.Factory
{
    public class MeshFactory : IMeshFactory
    {
        private readonly IAssetProvider _assets;
        private const int MaxVerticesForMeshCollider = 24000;

        public MeshFactory(IAssetProvider assets)
        {
            _assets = assets;
        }

        public void CreateFallingMesh(MeshData meshData,
            FallingMeshParticlePool fallingMeshFallingMeshParticlePool)
        {
            var mesh = new Mesh();
            mesh.indexFormat = meshData.IndexFormat;
            mesh.SetVertices(meshData.Vertices.ToArray());
            mesh.SetTriangles(meshData.Triangles, 0);
            mesh.SetColors(meshData.Colors);
            mesh.SetNormals(meshData.Normals);
            var fallingMesh = _assets.Instantiate(MeshPath.FallingMeshPath);
            fallingMesh.GetComponent<MeshFilter>().mesh = mesh;
            fallingMesh.GetComponent<FallingMesh>().Construct(fallingMeshFallingMeshParticlePool);
            var torque = new Vector3(Random.Range(0, 40), 0, Random.Range(0, 40));
            fallingMesh.GetComponent<Rigidbody>().AddTorque(torque);
            if (meshData.Vertices.Count > MaxVerticesForMeshCollider)
            {
                fallingMesh.AddComponent<SphereCollider>();
                var sphereCollider = fallingMesh.GetComponent<SphereCollider>();
                var meshRenderer = fallingMesh.GetComponent<MeshRenderer>().bounds;
                sphereCollider.center = meshRenderer.center;
                sphereCollider.radius = 1;
                sphereCollider.material = _assets.Load<PhysicMaterial>(MeshPath.PhysicMaterial);
            }
            else
            {
                fallingMesh.AddComponent<MeshCollider>();
                var meshCollider = fallingMesh.GetComponent<MeshCollider>();
                meshCollider.convex = true;
                meshCollider.sharedMesh = mesh;
                meshCollider.material = _assets.Load<PhysicMaterial>(MeshPath.PhysicMaterial);
            }
        }

        public GameObject CreateChunkMeshRender(Vector3 position, Quaternion rotation, Transform parent)
        {
            return _assets.Instantiate(MeshPath.ChunkMeshRendererPath, position, rotation, parent);
        }

        public void CreateWalls(MapProvider mapProvider, Transform parent)
        {
            var allFaces = Enum.GetValues(typeof(Faces)).Cast<Faces>().Where(face => face != Faces.None);
            foreach (var face in allFaces)
            {
                _assets.Instantiate(MeshPath.WallPath, parent).GetComponent<WallRenderer>()
                    .Construct(mapProvider, face);
            }
        }

        public void CreateGameModel(GameObject prefab, Transform itemPosition)
        {
            _assets.Instantiate(prefab, itemPosition);
        }

        public GameObject CreateTransparentTnt()
        {
            return _assets.Instantiate(MeshPath.TransparentTntPath);
        }

        public GameObject CreateTransparentBlock()
        {
            return _assets.Instantiate(MeshPath.TransparentBlockPath);
        }

        public void CreateWaterPlane(Vector3 position, Vector3 scale, Color32 waterColor)
        {
            var waterPlane = _assets.Instantiate(MeshPath.WaterPlane, position, Quaternion.identity);
            waterPlane.transform.localScale = scale;
            var mesh = new Mesh();
            mesh.SetVertices(new List<Vector3>()
                {new(-0.5f, 0, -0.5f), new(-0.5f, 0, 0.5f), new(0.5f, 0, -0.5f), new(0.5f, 0, 0.5f)});
            mesh.SetTriangles(new[] {0, 1, 2, 1, 3, 2}, 0);
            mesh.SetColors(Enumerable.Range(0, mesh.vertexCount).Select(_ => waterColor).ToList());
            mesh.SetNormals(Enumerable.Range(0, mesh.vertexCount).Select(_ => Vector3.up).ToList());
            waterPlane.GetComponent<MeshFilter>().sharedMesh = mesh;
        }
    }
}