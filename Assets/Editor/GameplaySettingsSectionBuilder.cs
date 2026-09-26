using TMPro;
using UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Editor
{
	// Builds the Gameplay tab of the settings menu by cloning the existing Controls tab and Video section.
	public static class GameplaySettingsSectionBuilder
	{
		private const string PrefabPath = "Assets/Resources/Prefabs/UI/SettingsMenu.prefab";
		private const float FirstRowY = -75;
		private const float RowStep = 75;
		private const float CheckboxSize = 40;
		private const float CheckboxX = 25;
		private const float CheckmarkInset = 12;

		[MenuItem("Tools/Build Gameplay Settings Section")]
		public static void Build()
		{
			GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
			try
			{
				SerializedObject settingsMenu = new SerializedObject(root.GetComponent<SettingsMenu>());
				if (settingsMenu.FindProperty("gameplaySection").objectReferenceValue != null)
				{
					EditorUtility.DisplayDialog("Gameplay settings", "Gameplay section is already built.", "OK");
					return;
				}

				Toggle sectionToggle = CreateSectionToggle(settingsMenu);
				GameObject section = CreateSection(settingsMenu, out Transform rowTemplate);
				Toggle autoJumpToggle = CreateCheckboxRow(rowTemplate, section.transform, "Auto jump", 0, true);
				Toggle autoReloadToggle = CreateCheckboxRow(rowTemplate, section.transform, "Auto reload", 1, false);
				Object.DestroyImmediate(rowTemplate.gameObject);

				settingsMenu.FindProperty("gameplaySectionToggle").objectReferenceValue = sectionToggle;
				settingsMenu.FindProperty("gameplaySection").objectReferenceValue = section;
				settingsMenu.FindProperty("autoJumpToggle").objectReferenceValue = autoJumpToggle;
				settingsMenu.FindProperty("autoReloadToggle").objectReferenceValue = autoReloadToggle;
				settingsMenu.ApplyModifiedPropertiesWithoutUndo();

				PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
				Debug.Log("Gameplay settings section built in " + PrefabPath);
			}
			finally
			{
				PrefabUtility.UnloadPrefabContents(root);
			}
		}

		private static Toggle CreateSectionToggle(SerializedObject settingsMenu)
		{
			Toggle controlsToggle = (Toggle)settingsMenu.FindProperty("controlsSectionToggle").objectReferenceValue;
			Toggle toggle = Object.Instantiate(controlsToggle, controlsToggle.transform.parent);
			toggle.name = "Gameplay section toggle";
			toggle.isOn = false;
			toggle.GetComponentInChildren<TMP_Text>().text = "Gameplay";
			return toggle;
		}

		private static GameObject CreateSection(SerializedObject settingsMenu, out Transform rowTemplate)
		{
			GameObject videoSection = (GameObject)settingsMenu.FindProperty("videoSection").objectReferenceValue;
			GameObject section = Object.Instantiate(videoSection, videoSection.transform.parent);
			section.name = "Gameplay section";
			section.SetActive(false);

			rowTemplate = section.transform.GetChild(0);
			for (int i = section.transform.childCount - 1; i > 0; i--)
			{
				Object.DestroyImmediate(section.transform.GetChild(i).gameObject);
			}

			return section;
		}

		private static Toggle CreateCheckboxRow(Transform template, Transform section, string label, int index, bool isOn)
		{
			Transform row = Object.Instantiate(template, section);
			row.name = label;
			RectTransform rowTransform = (RectTransform)row;
			rowTransform.anchoredPosition = new Vector2(rowTransform.anchoredPosition.x, FirstRowY - RowStep * index);

			foreach (MonoBehaviour component in row.GetComponents<MonoBehaviour>())
			{
				if (component is not Graphic)
				{
					Object.DestroyImmediate(component);
				}
			}

			TMP_Text settingName = row.Find("Setting name").GetComponent<TMP_Text>();
			settingName.text = label;

			ColorBlock buttonColors = row.Find("Decrease button").GetComponent<Button>().colors;
			Object.DestroyImmediate(row.Find("Decrease button").gameObject);
			Object.DestroyImmediate(row.Find("Increase button").gameObject);

			RectTransform field = (RectTransform)row.Find("Setting field image");
			field.name = "Checkbox";
			field.sizeDelta = new Vector2(CheckboxSize, CheckboxSize);
			field.anchoredPosition = new Vector2(CheckboxX, 0);
			for (int i = field.childCount - 1; i >= 0; i--)
			{
				Object.DestroyImmediate(field.GetChild(i).gameObject);
			}

			GameObject checkmark = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
			RectTransform checkmarkTransform = (RectTransform)checkmark.transform;
			checkmarkTransform.SetParent(field, false);
			checkmarkTransform.anchorMin = Vector2.zero;
			checkmarkTransform.anchorMax = Vector2.one;
			checkmarkTransform.sizeDelta = new Vector2(-CheckmarkInset, -CheckmarkInset);
			Image checkmarkImage = checkmark.GetComponent<Image>();
			checkmarkImage.color = settingName.color;
			checkmarkImage.raycastTarget = false;

			Toggle toggle = field.gameObject.AddComponent<Toggle>();
			toggle.targetGraphic = field.GetComponent<Image>();
			toggle.graphic = checkmarkImage;
			toggle.colors = buttonColors;
			toggle.isOn = isOn;
			return toggle;
		}
	}
}
