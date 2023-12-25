using System.Linq;
using Data;
using MapLogic;
using Networking;
using PlayerLogic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MiniMap : MonoBehaviour
    {
        [SerializeField]
        private RawImage minimapImage;

        private Texture2D _miniMapTexture;
        private MapProvider _mapProvider;
        private IClient _client;
        private Player _player;
        private int _chunkIndex;

        public void Construct(IClient client, Player player)
        {
            _client = client;
            _client.MapUpdated += OnMapUpdated;
            _mapProvider = _client.MapProvider;
            _player = player;
            _miniMapTexture = new Texture2D(ChunkData.ChunkSize, ChunkData.ChunkSize);
            _miniMapTexture.filterMode = FilterMode.Point;
        }

        private void Update()
        {
            var chunkIndex =
                _mapProvider.GetChunkNumberByGlobalPosition(Vector3Int.RoundToInt(_player.transform.position));
            if (_chunkIndex != chunkIndex)
            {
                RedrawMinimap(chunkIndex);
            }

            _chunkIndex = chunkIndex;
        }

        private void RedrawMinimap(int chunkIndex)
        {
            var pixels = new Color32[ChunkData.ChunkSizeSquared];
            for (var x = 0; x < ChunkData.ChunkSize; x++)
            {
                for (var z = 0; z < ChunkData.ChunkSize; z++)
                {
                    for (var y = _mapProvider.Height - 1; y >= 0; y--)
                    {
                        var block = _mapProvider.GetBlockByGlobalPosition(x + _mapProvider.GetChunkXOffset(chunkIndex),
                            y, z + _mapProvider.GetChunkZOffset(chunkIndex));
                        if (!block.IsSolid())
                        {
                            continue;
                        }

                        pixels[x * ChunkData.ChunkSize + z] = block.Color;
                        break;
                    }
                }
            }

            _miniMapTexture.SetPixels32(pixels);
            _miniMapTexture.Apply();
            minimapImage.texture = _miniMapTexture;
        }

        private void OnMapUpdated(BlockDataWithPosition[] blocks)
        {
            if (blocks.Any(block => _mapProvider.GetChunkNumberByGlobalPosition(block.Position) == _chunkIndex))
            {
                RedrawMinimap(_chunkIndex);
            }
        }

        private void OnDestroy()
        {
            _client.MapUpdated -= OnMapUpdated;
        }
    }
}