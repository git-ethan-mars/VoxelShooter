using System;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    public static class Bresenham3D
    {
        public static IEnumerable<Vector3Int> Calculate(Vector3Int startPoint, Vector3Int endPoint)
        {
            yield return startPoint;
            var dx = Math.Abs(endPoint.x - startPoint.x);
            var dy = Math.Abs(endPoint.y - startPoint.y);
            var dz = Math.Abs(endPoint.z - startPoint.z);
            var xs = endPoint.x.CompareTo(startPoint.x);
            var ys = endPoint.y.CompareTo(startPoint.y);
            var zs = endPoint.z.CompareTo(startPoint.z);
            // Driving axis is X-axis"
            if (dx >= dy && dx >= dz)
            {
                var p1 = 2 * dy - dx;
                var p2 = 2 * dz - dx;
                var x1 = startPoint.x;
                var x2 = endPoint.x;
                var y1 = startPoint.y;
                var z1 = startPoint.z;
                while (x1 != x2)
                {
                    x1 += xs;
                    if (p1 >= 0)
                    {
                        y1 += ys;
                        p1 -= 2 * dx;
                    }

                    if (p2 >= 0)
                    {
                        z1 += zs;
                        p2 -= 2 * dx;
                    }

                    p1 += 2 * dy;
                    p2 += 2 * dz;
                    yield return new Vector3Int(x1, y1, z1);
                }

                // Driving axis is Y-axis"
            }
            else if (dy >= dx && dy >= dz)
            {
                var p1 = 2 * dx - dy;
                var p2 = 2 * dz - dy;
                var x1 = startPoint.x;
                var y1 = startPoint.y;
                var y2 = endPoint.y;
                var z1 = startPoint.z;
                while (y1 != y2)
                {
                    y1 += ys;
                    if (p1 >= 0)
                    {
                        x1 += xs;
                        p1 -= 2 * dy;
                    }

                    if (p2 >= 0)
                    {
                        z1 += zs;
                        p2 -= 2 * dy;
                    }

                    p1 += 2 * dx;
                    p2 += 2 * dz;

                    yield return new Vector3Int(x1, y1, z1);
                }

                // Driving axis is Z-axis"
            }
            else
            {
                var p1 = 2 * dy - dz;
                var p2 = 2 * dx - dz;
                var x1 = startPoint.x;
                var y1 = startPoint.y;
                var z1 = startPoint.z;
                var z2 = endPoint.z;
                while (z1 != z2)
                {
                    z1 += zs;
                    if (p1 >= 0)
                    {
                        y1 += ys;
                        p1 -= 2 * dz;
                    }

                    if (p2 >= 0)
                    {
                        x1 += xs;
                        p2 -= 2 * dz;
                    }

                    p1 += 2 * dy;
                    p2 += 2 * dx;

                    yield return new Vector3Int(x1, y1, z1);
                }
            }
        }
    }
}