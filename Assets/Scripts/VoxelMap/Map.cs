using System;
using UnityEngine;

namespace VoxelMap
{
    public class Map : MonoBehaviour, IDisposable
    {
        public static readonly Vector3 WorldOffset = new Vector3(0.5f, 0.5f, 0.5f);
        public Light DirectionalLight { get; private set; }
        internal Chunk[] Chunks { get; private set; }
        internal MapData Data;
        private GameObject _waterLayer;
        private bool _isDisposed;

        internal void Construct(MapData data)
        {
            Data = data;
        }

        internal void SetDirectionalLight(Light directionalLight)
        {
            DirectionalLight = directionalLight;
        }

        internal void SetChunks(Chunk[] chunks)
        {
            Chunks = chunks;
        }
        
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            
            for (var i = 0; i < Chunks.Length; i++)
            {
                Chunks[i].Dispose();
            }

            _isDisposed = true;
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}