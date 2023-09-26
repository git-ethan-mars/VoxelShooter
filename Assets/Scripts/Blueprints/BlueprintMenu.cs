using System.Collections.Generic;
using Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Blueprints
{
    public class BlueprintMenu : MonoBehaviour
    {
        [SerializeField]
        private BlueprintBuilder builder;

        private const int TextureSize = 25;
        private const int TexturePadding = 5;
        private const int Margin = 10;
        private List<Texture2D> _textures;
        private GUIState _state;

        public void Awake()
        {
            _textures = new List<Texture2D>();
            LoadTextures();
        }

        public void OnGUI()
        {
            var boxStyle = new GUIStyle("box");
            if (_state == GUIState.Create)
            {
                GUILayout.BeginArea(new Rect(Margin, Margin, 255, 50), boxStyle);
                {
                    GUI.Button(new Rect(10, 10, 100, 30), "Load");
                    GUI.Button(new Rect(145, 10, 100, 30), "Create");
                }

                GUILayout.EndArea();
            }

            else
            {
                GUILayout.BeginArea(new Rect(Margin, Margin, 255, 300), boxStyle);
                {
                    GUILayout.Label("Palette");
                    foreach (var index in BlockColor.ColorById.Keys)
                    {
                        var rect = new Rect((TextureSize + TexturePadding) * ((index - 1) % 8) + 10,
                            (TextureSize + TexturePadding) * ((index - 1) / 8) + 25, TextureSize, TextureSize);
                        if (GUI.Button(rect, _textures[index - 1], GUIStyle.none))
                        {
                            builder.DesiredColor = BlockColor.ColorById[index];
                        }
                    }

                    GUI.Button(new Rect(10, 265, 100, 30), "Save");
                    GUI.Button(new Rect(145, 265, 100, 30), "Load");
                }
                GUILayout.EndArea();
            }
        }


        private void LoadTextures()
        {
            foreach (var color in BlockColor.ColorById.Values)
            {
                var texture = new Texture2D(TextureSize, TextureSize);
                for (var i = 0; i < TextureSize; i++)
                {
                    for (var j = 0; j < TextureSize; j++)
                    {
                        texture.SetPixel(i, j, color);
                    }
                }

                texture.Apply();
                _textures.Add(texture);
            }
        }


        private enum GUIState
        {
            Create,
            Build,
            Load
        }
    }
}