using System;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    public class DDA
    {
        public static IEnumerable<Vector3Int> Calculate(Vector3 startPoint, Vector3 endPoint)
        {
            // This id of the first/current voxel hit by the ray.
            // Using floor (round down) is actually very important,
            // the implicit int-casting will round up for negative numbers.
            var currentVoxel = Vector3Int.FloorToInt(startPoint);
            var endVoxel = Vector3Int.FloorToInt(endPoint);
            var ray = endPoint - startPoint;
            // In which direction the voxel ids are incremented.
            var stepX = (ray[0] >= 0) ? 1 : -1; // correct
            var stepY = (ray[1] >= 0) ? 1 : -1; // correct
            var stepZ = (ray[2] >= 0) ? 1 : -1; // correct
            // Distance along the ray to the next voxel border from the current position (tMaxX, tMaxY, tMaxZ).
            var next_voxel_boundary_x = (currentVoxel.x + stepX); // correct
            var next_voxel_boundary_y = (currentVoxel.y + stepY); // correct
            var next_voxel_boundary_z = (currentVoxel.z + stepZ); // correct
            // tMaxX, tMaxY, tMaxZ -- distance until next intersection with voxel-border
            // the value of t at which the ray crosses the first vertical voxel boundary
            var tMaxX = (ray[0] != 0) ? (next_voxel_boundary_x - startPoint[0]) / ray[0] : float.MaxValue; //
            var tMaxY = (ray[1] != 0) ? (next_voxel_boundary_y - startPoint[1]) / ray[1] : float.MaxValue; //
            var tMaxZ = (ray[2] != 0) ? (next_voxel_boundary_z - startPoint[2]) / ray[2] : float.MaxValue; //

            // tDeltaX, tDeltaY, tDeltaZ --
            // how far along the ray we must move for the horizontal component to equal the width of a voxel
            // the direction in which we traverse the grid
            // can only be FLT_MAX if we never go in that direction
            var tDeltaX = (ray[0] != 0) ? 1 / ray[0] * stepX : float.MaxValue;
            var tDeltaY = (ray[1] != 0) ? 1 / ray[1] * stepY : float.MaxValue;
            var tDeltaZ = (ray[2] != 0) ? 1 / ray[2] * stepZ : float.MaxValue;
            yield return Vector3Int.FloorToInt(currentVoxel);
            while (endVoxel != currentVoxel)
            {
                if (tMaxX < tMaxY)
                {
                    if (tMaxX < tMaxZ)
                    {
                        currentVoxel[0] += stepX;
                        tMaxX += tDeltaX;
                    }
                    else
                    {
                        currentVoxel[2] += stepZ;
                        tMaxZ += tDeltaZ;
                    }
                }
                else
                {
                    if (tMaxY < tMaxZ)
                    {
                        currentVoxel[1] += stepY;
                        tMaxY += tDeltaY;
                    }
                    else
                    {
                        currentVoxel[2] += stepZ;
                        tMaxZ += tDeltaZ;
                    }
                }

                yield return Vector3Int.FloorToInt(currentVoxel); 
            }
        }
    }
}