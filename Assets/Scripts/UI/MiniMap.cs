using MapLogic;
using Networking;
using PlayerLogic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MiniMap : MonoBehaviour
    {
        private const int MiniMapSize = 127;

        [SerializeField]
        private RawImage minimapImage;

        [SerializeField]
        private Image minimapCursor;

        private MapProvider _mapProvider;
        private Player _player;
        private Color32[] _miniMapPixels;
        private Texture2D _miniMapTexture;
        private Vector3Int _playerPosition;
        private Color32[] _fullMapPixels;

        public void Construct(IClient client, Player player)
        {
            _mapProvider = client.MapProvider;
            _fullMapPixels = client.MapProjector.Projection;
            _player = player;
            _miniMapPixels = new Color32[MiniMapSize * MiniMapSize];
            _miniMapTexture = new Texture2D(MiniMapSize, MiniMapSize);
            _miniMapTexture.filterMode = FilterMode.Point;
        }

        private void Update()
        {
            minimapCursor.transform.rotation = Quaternion.Euler(0, 0, -_player.BodyOrientation.rotation.eulerAngles.y);
            RedrawMinimap();
        }

        private void RedrawMinimap()
        {
            _playerPosition = Vector3Int.FloorToInt(_player.transform.position);
            for (var x = 0; x < MiniMapSize; x++)
            {
                for (var z = 0; z < MiniMapSize; z++)
                {
                    var xPosition = _playerPosition.x + x - MiniMapSize / 2;
                    var zPosition = _playerPosition.z + z - MiniMapSize / 2;
                    if (_mapProvider.IsInsideMap(xPosition, 0, zPosition))
                    {
                        _miniMapPixels[z * MiniMapSize + x] =
                            _fullMapPixels[zPosition * _mapProvider.Width + xPosition];
                    }
                    else
                    {
                        _miniMapPixels[z * MiniMapSize + x] = _mapProvider.WaterColor;
                    }
                }
            }

            _miniMapTexture.SetPixels32(_miniMapPixels);
            _miniMapTexture.Apply();
            minimapImage.texture = _miniMapTexture;
        }
    }
}