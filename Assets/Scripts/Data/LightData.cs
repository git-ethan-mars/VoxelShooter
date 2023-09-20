using System;
using CustomAttributes;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class LightData
    {
        [ReadOnly]
        public Vector3 position;

        [ReadOnly]
        public Quaternion rotation;

        [ReadOnly]
        public Color color;

        public LightData(Vector3 position, Quaternion rotation, Color color)
        {
            this.position = position;
            this.rotation = rotation;
            this.color = color;
        }
    }
}