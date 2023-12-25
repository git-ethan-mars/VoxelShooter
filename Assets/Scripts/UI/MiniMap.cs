using System.Collections;
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
        private const int MiniMapSize = 31;
        private const float MapUpdateRateInSeconds = 0.5f;

        [SerializeField]
        private RawImage minimapImage;

        [SerializeField]
        private Image minimapCursor;

        private Texture2D _miniMapTexture;
        private MapProvider _mapProvider;
        private IClient _client;
        private Player _player;
        private Color32[] _pixels;
        private Vector3Int _playerPosition;

        public void Construct(IClient client, Player player)
        {
            _client = client;
            _mapProvider = _client.MapProvider;
            _player = player;
            _miniMapTexture = new Texture2D(MiniMapSize, MiniMapSize);
            _pixels = new Color32[MiniMapSize * MiniMapSize];
            _miniMapTexture.filterMode = FilterMode.Point;
            StartCoroutine(RedrawMinimap());
        }

        private void Update()
        {
            minimapCursor.transform.rotation = Quaternion.Euler(0, 0, -_player.BodyOrientation.rotation.eulerAngles.y);
        }

        private IEnumerator RedrawMinimap()
        {
            while (true)
            {
                _playerPosition = Vector3Int.FloorToInt(_player.transform.position);
                for (var x = 0; x < MiniMapSize; x++)
                {
                    for (var z = 0; z < MiniMapSize; z++)
                    {
                        for (var y = _mapProvider.Height - 1; y >= 0; y--)
                        {
                            var xPosition = _playerPosition.x + x - MiniMapSize / 2;
                            var zPosition = _playerPosition.z + z - MiniMapSize / 2;
                            BlockData block;
                            if (_mapProvider.IsInsideMap(xPosition, y, zPosition))
                            {
                                block = _mapProvider.GetBlockByGlobalPosition(xPosition,
                                    y, zPosition);

                                if (!block.IsSolid())
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                block = new BlockData(_mapProvider.WaterColor);
                            }

                            _pixels[z * MiniMapSize + x] = block.Color;
                            break;
                        }
                    }
                }

                _miniMapTexture.SetPixels32(_pixels);
                _miniMapTexture.Apply();
                minimapImage.texture = _miniMapTexture;
                yield return new WaitForSeconds(MapUpdateRateInSeconds);
            }
        }
    }
}