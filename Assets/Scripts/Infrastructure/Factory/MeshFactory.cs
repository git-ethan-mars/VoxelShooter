using System.Linq;
using Infrastructure.AssetManagement;
using UnityEngine;

namespace Infrastructure.Factory
{
    public class MeshFactory : IMeshFactory
    {
        private const string FallingMesh = "Prefabs/MapCreation/FallingMesh";
        private readonly IAssetProvider _assets;

        public MeshFactory(IAssetProvider assets)
        {
            _assets = assets;
        }

        public void CreateFallingMesh(Rendering.MeshData meshData)
        {
            var mesh = new Mesh();
            mesh.SetVertices(meshData.Vertices.ToArray());
            mesh.SetTriangles(meshData.Triangles, 0);
            mesh.SetColors(meshData.Colors);
            mesh.SetNormals(meshData.Normals);
            mesh.indexFormat = meshData.IndexFormat;
            var fallingMesh = _assets.Instantiate(FallingMesh);
            fallingMesh.GetComponent<MeshFilter>().mesh = mesh;
            var meshCollider = fallingMesh.GetComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
        }

        public GameObject CreateGameModel(GameObject prefab, Transform itemPosition)
        {
            return _assets.Instantiate(prefab, itemPosition);
        }
    }
}