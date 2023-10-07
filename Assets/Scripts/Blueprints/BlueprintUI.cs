using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;

namespace Blueprints
{
    public class BlueprintUI
    {
        public BlueprintGUIState State { get; private set; }
        private const int TextureSize = 25;
        private const int TexturePadding = 5;
        private const int Margin = 10;
        private Vector2 _scrollPosition;
        private readonly List<Texture2D> _textures;
        private readonly BlueprintBuilder _builder;
        private int WindowSize => Screen.width / 5;

        public BlueprintUI(BlueprintBuilder builder)
        {
            _builder = builder;
            _textures = new List<Texture2D>();
            LoadTextures();
        }

        public void DrawCreateMenu()
        {
            var boxStyle = new GUIStyle("box");
            boxStyle.fontSize = WindowSize / 10;
            boxStyle.alignment = TextAnchor.MiddleCenter;
            GUILayout.BeginArea(new Rect(Margin, Margin, WindowSize, WindowSize), boxStyle);
            {
                if (GUI.Button(new Rect(WindowSize / 9, WindowSize / 9, 7 * WindowSize / 9, WindowSize / 3), "Load",
                        boxStyle))
                {
                    State = BlueprintGUIState.Choose;
                }

                if (GUI.Button(new Rect(WindowSize / 9, 5 * WindowSize / 9, 7 * WindowSize / 9, WindowSize / 3),
                        "Create", boxStyle))
                {
                    _builder.CreateNewBlueprint();
                    State = BlueprintGUIState.Build;
                }
            }

            GUILayout.EndArea();
        }

        public void DrawChooseMenu()
        {
            var blueprintNames = BlueprintLoader.LoadBlueprintFiles().ToArray();
            var boxStyle = new GUIStyle("box");
            boxStyle.fontSize = WindowSize / 10;
            boxStyle.alignment = TextAnchor.MiddleCenter;
            GUILayout.BeginArea(new Rect(Margin, Margin, WindowSize, WindowSize), boxStyle);
            _scrollPosition = GUILayout.BeginScrollView(
                _scrollPosition, GUILayout.Width(WindowSize), GUILayout.Height(WindowSize));
            foreach (var blueprintName in blueprintNames)
            {
                if (GUILayout.Button(blueprintName))
                {
                    _builder.CreateExistedBlueprint(BlueprintLoader.GetBlueprintData(blueprintName));
                }
            }

            if (GUI.Button(new Rect(),"Back"))
            {
                State = BlueprintGUIState.Create;
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        public void DrawBuildMenu()
        {
            var boxStyle = new GUIStyle("box");
            GUILayout.BeginArea(new Rect(Margin, Margin, 255, 300), boxStyle);
            {
                GUILayout.Label("Palette");
                foreach (var index in BlockColor.ColorById.Keys)
                {
                    var rect = new Rect((TextureSize + TexturePadding) * ((index - 1) % 8) + 10,
                        // ReSharper disable once PossibleLossOfFraction
                        (TextureSize + TexturePadding) * ((index - 1) / 8) + 25, TextureSize, TextureSize);
                    if (GUI.Button(rect, _textures[index - 1], GUIStyle.none))
                    {
                        _builder.DesiredColor = BlockColor.ColorById[index];
                    }
                }

                if (GUI.Button(new Rect(10, 265, 100, 30), "Save"))
                {
                }

                if (GUI.Button(new Rect(145, 265, 100, 30), "Load"))
                {
                }
            }
            GUILayout.EndArea();
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
    }
}