using Data;
using TMPro;
using UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Editor
{
	// Adds the live map panorama to the game menu and a Minecraft-like splash to the main menu.
	public static class MainMenuPanoramaBuilder
	{
		private const string GameMenuPath = "Assets/Resources/Prefabs/UI/GameMenu.prefab";
		private const string MainMenuPath = "Assets/Resources/Prefabs/UI/MainMenu.prefab";
		private const string SplashTextsPath = "Assets/Resources/StaticData/Splash Texts.asset";
		private static readonly Vector2 SplashPosition = new Vector2(340, 290);
		private static readonly Vector2 SplashSize = new Vector2(520, 80);
		private static readonly Color SplashColor = new Color(1.0f, 1.0f, 0.2f);
		private const float SplashAngle = 20;
		private const float SplashFontSize = 42;

		[MenuItem("Tools/Build Main Menu Panorama And Splash")]
		public static void Build()
		{
			AddPanorama();
			AddSplash();
			Debug.Log("Main menu panorama and splash are built");
		}

		private static void AddPanorama()
		{
			GameObject root = PrefabUtility.LoadPrefabContents(GameMenuPath);
			try
			{
				if (root.GetComponent<MainMenuPanorama>() != null)
				{
					return;
				}

				Image backgroundImage = (Image)new SerializedObject(root.GetComponent<GameMenu>())
					.FindProperty("backgroundImage").objectReferenceValue;
				SerializedObject panorama = new SerializedObject(root.AddComponent<MainMenuPanorama>());
				panorama.FindProperty("backgroundImage").objectReferenceValue = backgroundImage;
				panorama.ApplyModifiedPropertiesWithoutUndo();

				PrefabUtility.SaveAsPrefabAsset(root, GameMenuPath);
			}
			finally
			{
				PrefabUtility.UnloadPrefabContents(root);
			}
		}

		private static void AddSplash()
		{
			GameObject root = PrefabUtility.LoadPrefabContents(MainMenuPath);
			try
			{
				if (root.GetComponentInChildren<SplashText>(true) != null)
				{
					return;
				}

				TMP_Text buttonText = root.GetComponentInChildren<TMP_Text>(true);

				GameObject splash = new GameObject("Splash", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
				RectTransform splashTransform = (RectTransform)splash.transform;
				splashTransform.SetParent(root.transform, false);
				splashTransform.SetAsLastSibling();
				splashTransform.anchorMin = new Vector2(0.5f, 0.5f);
				splashTransform.anchorMax = new Vector2(0.5f, 0.5f);
				splashTransform.anchoredPosition = SplashPosition;
				splashTransform.sizeDelta = SplashSize;
				splashTransform.localRotation = Quaternion.Euler(0, 0, SplashAngle);

				TextMeshProUGUI text = splash.GetComponent<TextMeshProUGUI>();
				text.font = buttonText.font;
				text.fontSharedMaterial = buttonText.fontSharedMaterial;
				text.fontSize = SplashFontSize;
				text.fontStyle = FontStyles.Bold;
				text.color = SplashColor;
				text.alignment = TextAlignmentOptions.Center;
				text.textWrappingMode = TextWrappingModes.NoWrap;
				text.raycastTarget = false;
				text.text = "Splash!";

				SerializedObject splashText = new SerializedObject(splash.AddComponent<SplashText>());
				splashText.FindProperty("text").objectReferenceValue = text;
				splashText.FindProperty("splashTexts").objectReferenceValue =
					AssetDatabase.LoadAssetAtPath<SplashTextsConfigure>(SplashTextsPath);
				splashText.ApplyModifiedPropertiesWithoutUndo();

				PrefabUtility.SaveAsPrefabAsset(root, MainMenuPath);
			}
			finally
			{
				PrefabUtility.UnloadPrefabContents(root);
			}
		}
	}
}
