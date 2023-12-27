using System.Collections.Generic;
using Entities;
using Infrastructure.Factory;
using MapLogic;
using Networking;
using PlayerLogic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MiniMap : MonoBehaviour
    {
        private const int MiniMapSize = 250;

        [SerializeField]
        private RawImage minimapImage;

        [SerializeField]
        private Image minimapCursor;

        private List<Image> _lootBoxImages = new();

        private MapProvider _mapProvider;
        private Player _player;
        private UIFactory _uiFactory;
        private Color32[] _miniMapPixels;
        private Texture2D _miniMapTexture;
        private Color32[] _fullMapPixels;
        private HashSet<LootBox> _lootBoxes;

        public void Construct(IClient client, Player player, UIFactory uiFactory)
        {
            _mapProvider = client.MapProvider;
            _fullMapPixels = client.MapProjector.Projection;
            _lootBoxes = client.LootBoxes;
            _player = player;
            _uiFactory = uiFactory;
            _miniMapPixels = new Color32[MiniMapSize * MiniMapSize];
            _miniMapTexture = new Texture2D(MiniMapSize, MiniMapSize);
            _miniMapTexture.filterMode = FilterMode.Point;
        }

        private void Update()
        {
            RedrawCursor();
            RedrawMinimap();
            RedrawLootBoxes();
        }

        private void RedrawCursor()
        {
            minimapCursor.transform.rotation = Quaternion.Euler(0, 0, -_player.BodyOrientation.rotation.eulerAngles.y);
        }

        private void RedrawMinimap()
        {
            var playerPosition = Vector3Int.FloorToInt(_player.transform.position);
            for (var x = 0; x < MiniMapSize; x++)
            {
                for (var z = 0; z < MiniMapSize; z++)
                {
                    var xPosition = playerPosition.x + x - MiniMapSize / 2;
                    var zPosition = playerPosition.z + z - MiniMapSize / 2;
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

        private void RedrawLootBoxes()
        {
            var playerPosition = Vector3Int.FloorToInt(_player.transform.position);
            var lootBoxesToDraw = 0;
            for (var i = 0; i < _lootBoxImages.Count; i++)
            {
                _lootBoxImages[i].gameObject.SetActive(false);
            }

            foreach (var lootBox in _lootBoxes)
            {
                var lootBoxPosition = Vector3Int.FloorToInt(lootBox.transform.position);
                if (Mathf.Abs(playerPosition.x - lootBoxPosition.x) < MiniMapSize / 2 &&
                    Mathf.Abs(playerPosition.z - lootBoxPosition.z) < MiniMapSize / 2)
                {
                    lootBoxesToDraw += 1;
                    if (lootBoxesToDraw > _lootBoxImages.Count)
                    {
                        _lootBoxImages.Add(_uiFactory.CreateLootBoxImage(transform));
                    }

                    var imageIndex = lootBoxesToDraw - 1;
                    _lootBoxImages[imageIndex].transform.localPosition =
                        new Vector3(lootBoxPosition.x - playerPosition.x,
                            lootBoxPosition.z - playerPosition.z);
                    _lootBoxImages[imageIndex].sprite = lootBox.MiniMapImage;
                    _lootBoxImages[imageIndex].gameObject.SetActive(true);
                }
            }
        }
    }
}