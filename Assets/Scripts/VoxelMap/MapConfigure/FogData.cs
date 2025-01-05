using System;
using UnityEngine;
using VoxelMap.CustomAttributes;

namespace VoxelMap
{
    [Serializable]
    public class FogData
    {
        [ReadOnly]
        public bool activated;

        [ReadOnly]
        public FogMode mode;

        [ReadOnly]
        public Color color;

        [ReadOnly]
        public float startDistance;

        [ReadOnly]
        public float endDistance;

        [ReadOnly]
        public float density;

        public FogData(bool activated, FogMode mode, Color color, float startDistance, float endDistance, float density)
        {
            this.activated = activated;
            this.mode = mode;
            this.color = color;
            this.startDistance = startDistance;
            this.endDistance = endDistance;
            this.density = density;
        }
    }
}