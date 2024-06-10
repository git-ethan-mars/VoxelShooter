using System;
using System.Buffers;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    public static class VoxelLineDrawer
    {
        public static IEnumerable<Vector3Int> Calculate(Vector3Int startPoint, Vector3Int endPoint, Connectivity connectivity)
        {
            var deltasX = ArrayPool<int>.Shared.Rent(2);
            var deltasY = ArrayPool<int>.Shared.Rent(2);
            var deltasZ = ArrayPool<int>.Shared.Rent(2);
            UpdateDeltas(startPoint.x, endPoint.x, deltasX);
            UpdateDeltas(startPoint.y, endPoint.y, deltasY);
            UpdateDeltas(startPoint.z, endPoint.z, deltasZ);
            yield return startPoint;
            var currentPoint = startPoint;
            var ray = new Ray(startPoint, endPoint - startPoint);
            while (currentPoint != endPoint)
            {
                currentPoint = GetClosestPoint(currentPoint, ray, deltasX, deltasY, deltasZ, connectivity);
                yield return currentPoint;
            }

            ArrayPool<int>.Shared.Return(deltasX);
            ArrayPool<int>.Shared.Return(deltasY);
            ArrayPool<int>.Shared.Return(deltasZ);
        }

        private static Vector3Int GetClosestPoint(Vector3Int centralPoint, Ray ray, int[] deltasX,
            int[] deltasY, int[] deltasZ, Connectivity connectivity)
        {
            var bestDistance = Mathf.Infinity;
            var result = centralPoint;
            for (var i = 0; i < 2; i++)
            {
                for (var j = 0; j < 2; j++)
                {
                    for (var k = 0; k < 2; k++)
                    {
                        var x = deltasX[i];
                        var y = deltasY[j];
                        var z = deltasZ[k];
                        if (x == 0 && y == 0 && z == 0)
                        {
                            continue;
                        }

                        if (connectivity == Connectivity.Four && Math.Abs(x) + Math.Abs(y) + Math.Abs(z) != 1)
                        {
                            continue;
                        }

                        var point = centralPoint + new Vector3Int(x, y, z);
                        var distance = Vector3.Cross(ray.direction, point - ray.origin).magnitude /
                                       ray.direction.magnitude;
                        if (distance < bestDistance)
                        {
                            bestDistance = distance;
                            result = point;
                        }
                    }
                }
            }

            return result;
        }

        private static void UpdateDeltas(int startCoordinate, int endCoordinate, int[] deltas)
        {
            deltas[0] = 0;
            deltas[1] = endCoordinate.CompareTo(startCoordinate);
        }
    
        public enum Connectivity
        {
            Four,
            Eight
        }
    }
}