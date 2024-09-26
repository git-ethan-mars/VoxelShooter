using Common.AssetManagement;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Common.Factory
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
            FallingMeshParticlePool fallingMeshParticlePool)
        {
            var mesh = new Mesh();
            mesh.indexFormat = meshData.IndexFormat;
            mesh.SetVertices(meshData.Vertices.ToArray());
            mesh.SetTriangles(meshData.Triangles, 0);
            mesh.SetColors(meshData.Colors);
            mesh.SetNormals(meshData.Normals);
            var fallingMesh = _assets.Instantiate(MeshPath.FallingMeshPath);
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

        public void CreateGameModel(GameObject prefab, Transform itemPosition)
        {
            _assets.Instantiate(prefab, itemPosition);
        }

        public GameObject CreateTransparentTnt()
        {
            return _assets.Instantiate(MeshPath.TransparentTntPath);
        }
        public GameObject CreatTransparentBlock()
        {
            return _assets.Instantiate(MeshPath.TransparentBlockPath);
        }
    }
}