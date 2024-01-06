using Data;
using MapLogic;
using UnityEngine;

namespace Rendering
{
    public class WallRenderer : MonoBehaviour
    {
        [SerializeField]
        private MeshFilter meshFilter;

        [SerializeField]
        private MeshCollider meshCollider;
        public void Construct(MapProvider mapProvider, Faces face)
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[4];
            if (face == Faces.Top)
            {
                var startPoint = new Vector3(0, mapProvider.Height, 0);
                var endPoint = new Vector3(mapProvider.Width, mapProvider.Height,
                    mapProvider.Depth);
                var secondPoint = new Vector3(0, mapProvider.Height, mapProvider.Depth);
                var thirdPoint = new Vector3(mapProvider.Width, mapProvider.Height, 0);
                mesh.vertices = new[] {startPoint, secondPoint, thirdPoint, endPoint};
                mesh.triangles = new[] {0, 2, 1, 2, 3, 1};
            }

            if (face == Faces.Bottom)
            {
                var startPoint = new Vector3(0, 0, 0);
                var endPoint = new Vector3(mapProvider.Width, 0, mapProvider.Depth);
                var secondPoint = new Vector3(0, 0, mapProvider.Depth);
                var thirdPoint = new Vector3(mapProvider.Width, 0, 0);
                mesh.vertices = new[] {startPoint, secondPoint, thirdPoint, endPoint};
                mesh.triangles = new[] {0, 1, 2, 3, 2, 1};
            }

            if (face == Faces.Left)
            {
                var startPoint = new Vector3(0, 0, 0);
                var endPoint = new Vector3(0, mapProvider.Height, mapProvider.Depth);
                var secondPoint = new Vector3(0, 0, mapProvider.Depth);
                var thirdPoint = new Vector3(0, mapProvider.Height, 0);
                mesh.vertices = new[] {startPoint, secondPoint, thirdPoint, endPoint};
                mesh.triangles = new[] {0, 2, 1, 2, 3, 1};
            }

            if (face == Faces.Right)
            {
                var startPoint = new Vector3(mapProvider.Width, 0, 0);
                var endPoint = new Vector3(mapProvider.Width, mapProvider.Height,
                    mapProvider.Depth);
                var secondPoint = new Vector3(mapProvider.Width, 0, mapProvider.Depth);
                var thirdPoint = new Vector3(mapProvider.Width, mapProvider.Height, 0);
                mesh.vertices = new[] {startPoint, secondPoint, thirdPoint, endPoint};
                mesh.triangles = new[] {0, 1, 2, 3, 2, 1};
            }

            if (face == Faces.Front)
            {
                var startPoint = new Vector3(0, 0, mapProvider.Depth);
                var endPoint = new Vector3(mapProvider.Width, mapProvider.Height,
                    mapProvider.Depth);
                var secondPoint = new Vector3(mapProvider.Width, 0, mapProvider.Depth);
                var thirdPoint = new Vector3(0, mapProvider.Height, mapProvider.Depth);
                mesh.vertices = new[] {startPoint, secondPoint, thirdPoint, endPoint};
                mesh.triangles = new[] {0, 2, 1, 2, 3, 1};
            }

            if (face == Faces.Back)
            {
                var startPoint = new Vector3(0, 0, 0);
                var endPoint = new Vector3(mapProvider.Width, mapProvider.Height, 0);
                var secondPoint = new Vector3(mapProvider.Width, 0, 0);
                var thirdPoint = new Vector3(0, mapProvider.Height, 0);
                mesh.vertices = new[] {startPoint, secondPoint, thirdPoint, endPoint};
                mesh.triangles = new[] {0, 1, 2, 3, 2, 1};
            }

            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
        }
    }
}