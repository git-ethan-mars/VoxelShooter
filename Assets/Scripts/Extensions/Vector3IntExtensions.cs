using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
    public static class Vector3IntExtensions
    {
        public static List<Vector3Int> GetNeighbours(this Vector3Int vector)
        {
            var neighbours = new List<Vector3Int>();
            neighbours.Add(new Vector3Int(-1, 0, 0) + vector);
            neighbours.Add(new Vector3Int(1, 0, 0) + vector);
            neighbours.Add(new Vector3Int(0, -1, 0) + vector);
            neighbours.Add(new Vector3Int(0, 1, 0) + vector);
            neighbours.Add(new Vector3Int(0, 0, -1) + vector);
            neighbours.Add(new Vector3Int(0, 0, 1) + vector);
            return neighbours;
        }
    }
}