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

        [ReadOnly]
        public float bias;

        [ReadOnly]
        public float normalBias;

        public LightData(Vector3 position, Quaternion rotation, Color color, float bias, float normalBias)
        {
            this.position = position;
            this.rotation = rotation;
            this.color = color;
            this.bias = bias;
            this.normalBias = normalBias;
        }
    }
}