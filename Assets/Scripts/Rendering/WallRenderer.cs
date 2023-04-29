using Data;
using MapLogic;
using UnityEngine;

namespace Rendering
{
    public class WallRenderer : MonoBehaviour
    {

        public void Construct(Map map, Faces face)
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[4];
            if (face == Faces.Top)
            {
                var startPoint = new Vector3(0, map.MapData.Height, 0);
                var endPoint = new Vector3(map.MapData.Width, map.MapData.Height, map.MapData.Depth);
                var secondPoint = new Vector3(0, map.MapData.Height, map.MapData.Depth);
                var thirdPoint = new Vector3(map.MapData.Width, map.MapData.Height, 0);
                mesh.vertices = new[] {startPoint, secondPoint, thirdPoint, endPoint};
                mesh.triangles = new[] {0, 2, 1, 2, 3, 1};
            }

            if (face == Faces.Bottom)
            {
                var startPoint = new Vector3(0, 0, 0);
                var endPoint = new Vector3(map.MapData.Width, 0, map.MapData.Depth);
                var secondPoint = new Vector3(0, 0, map.MapData.Depth);
                var thirdPoint = new Vector3(map.MapData.Width, 0, 0);
                mesh.vertices = new[] {startPoint, secondPoint, thirdPoint, endPoint};
                mesh.triangles = new[] {0, 1, 2, 3, 2, 1};
            }

            if (face == Faces.Left)
            {
                var startPoint = new Vector3(0, 0, 0);
                var endPoint = new Vector3(0, map.MapData.Height, map.MapData.Depth);
                var secondPoint = new Vector3(0, 0, map.MapData.Depth);
                var thirdPoint = new Vector3(0, map.MapData.Height, 0);
                mesh.vertices = new[] {startPoint, secondPoint, thirdPoint, endPoint};
                mesh.triangles = new[] {0, 2, 1, 2, 3, 1};
            }

            if (face == Faces.Right)
            {
                var startPoint = new Vector3(map.MapData.Width, 0, 0);
                var endPoint = new Vector3(map.MapData.Width, map.MapData.Height, map.MapData.Depth);
                var secondPoint = new Vector3(map.MapData.Width, 0, map.MapData.Depth);
                var thirdPoint = new Vector3(map.MapData.Width, map.MapData.Height, 0);
                mesh.vertices = new[] {startPoint, secondPoint, thirdPoint, endPoint};
                mesh.triangles = new[] {0, 1, 2, 3, 2, 1};

            }

            if (face == Faces.Front)
            {
                var startPoint = new Vector3(0, 0, map.MapData.Depth);
                var endPoint = new Vector3(map.MapData.Width, map.MapData.Height, map.MapData.Depth);
                var secondPoint = new Vector3(map.MapData.Width, 0, map.MapData.Depth);
                var thirdPoint = new Vector3(0, map.MapData.Height, map.MapData.Depth);
                mesh.vertices = new[] {startPoint, secondPoint, thirdPoint, endPoint};
                mesh.triangles = new[] {0, 2, 1, 2, 3, 1};
                
            }

            if (face == Faces.Back)
            {
                var startPoint = new Vector3(0, 0, 0);
                var endPoint = new Vector3(map.MapData.Width, map.MapData.Height, 0);
                var secondPoint = new Vector3(map.MapData.Width, 0, 0);
                var thirdPoint = new Vector3(0, map.MapData.Height, 0);
                mesh.vertices = new[] {startPoint, secondPoint, thirdPoint, endPoint};
                mesh.triangles = new[] {0, 1, 2, 3, 2, 1};
            }

            GetComponent<MeshFilter>().mesh = mesh;
            GetComponent<MeshCollider>().sharedMesh = mesh;
        }
    }
}
