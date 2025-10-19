using Data;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using VoxelMap;
using Environment = VoxelMap.Environment;
namespace Editor
{
	public class MapEditorWindow : EditorWindow
	{
		private readonly string[] _ambientModes = { "Skybox", "Gradient", "Color" };
		private readonly string[] _fogModes = { "Linear", "Exponential", "Exponential squared" };

		private ReorderableList _spawnPoints;
		private SerializedObject _serializedObject;

		public void Setup(MapConfigure mapConfigure)
		{
			_serializedObject = new SerializedObject(mapConfigure);
			_spawnPoints = new ReorderableList(_serializedObject, _serializedObject.FindProperty("SpawnPoints"),
				false,
				true,
				true, true)
			{
				drawHeaderCallback = DrawHeader,
				drawElementCallback = DrawListItems,
				onAddCallback = AddItem,
				onRemoveCallback = RemoveItem
			};
		}

		private void OnDisable()
		{
			_serializedObject.Dispose();
		}

		private void OnGUI()
		{
			_serializedObject.Update();
			DrawGUI();
			_serializedObject.ApplyModifiedProperties();
		}

		private void DrawGUI()
		{
			var imageProperty = _serializedObject.FindProperty(GetBackingField(nameof(MapConfigure.Image)));
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.ObjectField(imageProperty, new GUIContent(imageProperty.displayName));

			if (EditorGUI.EndChangeCheck())
			{
				_serializedObject.ApplyModifiedProperties();
			}
			
			DrawColorBlocks();
			DrawAmbientProperties();
			DrawFogProperties();
			DrawWeatherProperty();

			_spawnPoints.DoLayoutList();

			if (EditorUtility.IsDirty(_serializedObject.targetObject) && GUILayout.Button("Save configure"))
			{
				AssetDatabase.SaveAssets();
			}
		}

		private void DrawColorBlocks()
		{
			EditorGUILayout.LabelField("Color blocks");
			EditorGUI.indentLevel += 1;
			var waterColorProperty = _serializedObject.FindProperty(GetBackingField(nameof(MapConfigure.WaterColor)));
			waterColorProperty.colorValue = EditorGUILayout.ColorField(waterColorProperty.displayName,
				waterColorProperty.colorValue);

			var innerColorProperty = _serializedObject.FindProperty(GetBackingField(nameof(MapConfigure.InnerColor)));
			innerColorProperty.colorValue = EditorGUILayout.ColorField(new GUIContent(innerColorProperty.displayName),
				innerColorProperty.colorValue);

			EditorGUI.indentLevel -= 1;
		}

		private void DrawWeatherProperty()
		{
			var weatherProperty = _serializedObject.FindProperty(GetBackingField(nameof(MapConfigure.Weather)));
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.ObjectField(weatherProperty, new GUIContent(weatherProperty.displayName));
			if (EditorGUI.EndChangeCheck())
			{
				_serializedObject.ApplyModifiedProperties();
			}
		}

		private void DrawFogProperties()
		{
			var fogDataProperty = _serializedObject.FindProperty(GetBackingField(nameof(MapConfigure.FogData)));
			var fogActivatedProperty = fogDataProperty.FindPropertyRelative(nameof(MapConfigure.FogData.activated));
			EditorGUI.BeginChangeCheck();
			var isFogActivated =
				EditorGUILayout.Toggle("Fog", fogActivatedProperty.boolValue);

			fogActivatedProperty.boolValue = isFogActivated;
			EditorGUI.indentLevel += 1;
			if (isFogActivated)
			{
				var fogColorProperty = fogDataProperty.FindPropertyRelative(nameof(MapConfigure.FogData.color));
				fogColorProperty.colorValue = EditorGUILayout.ColorField(fogColorProperty.displayName, fogColorProperty.colorValue);

				var fogModeProperty = fogDataProperty.FindPropertyRelative(nameof(MapConfigure.FogData.mode));
				fogModeProperty.intValue = EditorGUILayout.IntPopup(fogModeProperty.displayName, fogModeProperty.intValue,
					_fogModes, new[] { 1, 2, 3 });

				if ((FogMode)fogModeProperty.intValue != FogMode.Linear)
				{
					var fogDensityProperty = fogDataProperty.FindPropertyRelative(nameof(MapConfigure.FogData.density));
					fogDensityProperty.floatValue = EditorGUILayout.FloatField(fogDensityProperty.displayName,
						fogDensityProperty.floatValue);
				}
				else
				{
					var fogStartDistanceProperty = fogDataProperty.FindPropertyRelative(nameof(MapConfigure.FogData.startDistance));
					fogStartDistanceProperty.floatValue = EditorGUILayout.FloatField(fogStartDistanceProperty.displayName,
						fogStartDistanceProperty.floatValue);

					var fogEndDistanceProperty = fogDataProperty.FindPropertyRelative(nameof(MapConfigure.FogData.endDistance));
					fogEndDistanceProperty.floatValue = EditorGUILayout.FloatField(fogEndDistanceProperty.displayName,
						fogEndDistanceProperty.floatValue);
				}
			}

			EditorGUI.indentLevel -= 1;
			_serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck())
			{
				Environment.ApplyFog((FogData)fogDataProperty.managedReferenceValue);
			}
		}

		private void DrawAmbientProperties()
		{
			EditorGUILayout.LabelField("Ambient settings");
			EditorGUI.BeginChangeCheck();
			EditorGUI.indentLevel += 1;
			var skyboxMaterialProperty = _serializedObject.FindProperty(GetBackingField(nameof(MapConfigure.SkyboxMaterial)));
			EditorGUILayout.ObjectField(skyboxMaterialProperty, new GUIContent(skyboxMaterialProperty.displayName));
			var ambientDataProperty = _serializedObject.FindProperty(GetBackingField(nameof(MapConfigure.AmbientData)));

			var ambientModeProperty = ambientDataProperty.FindPropertyRelative(nameof(MapConfigure.AmbientData.mode));
			ambientModeProperty.intValue =
				EditorGUILayout.IntPopup("Source", ambientModeProperty.intValue, _ambientModes, new[] { 0, 1, 3 });

			var skyColorProperty = ambientDataProperty.FindPropertyRelative(nameof(MapConfigure.AmbientData.skyColor));
			var equatorColorProperty = ambientDataProperty.FindPropertyRelative(nameof(MapConfigure.AmbientData.equatorColor));
			var groundColorProperty = ambientDataProperty.FindPropertyRelative(nameof(MapConfigure.AmbientData.groundColor));

			if ((AmbientMode)ambientModeProperty.intValue == AmbientMode.Skybox)
			{
				if (skyboxMaterialProperty.objectReferenceValue == null)
				{
					skyColorProperty.colorValue = EditorGUILayout.ColorField(skyColorProperty.displayName, skyColorProperty.colorValue);
				}
				else
				{
					var ambientIntensityProperty = ambientDataProperty.FindPropertyRelative(nameof(MapConfigure.AmbientData.intensity));
					ambientIntensityProperty.floatValue =
						EditorGUILayout.Slider("Intensity Multiplier",
							ambientIntensityProperty.floatValue, 0.0F, 8.0F);
				}
			}
			else if ((AmbientMode)ambientModeProperty.intValue == AmbientMode.Trilight)
			{
				skyColorProperty.colorValue = EditorGUILayout.ColorField(skyColorProperty.displayName, skyColorProperty.colorValue);
				equatorColorProperty.colorValue = EditorGUILayout.ColorField(equatorColorProperty.displayName,
					equatorColorProperty.colorValue);

				groundColorProperty.colorValue = EditorGUILayout.ColorField(groundColorProperty.displayName,
					groundColorProperty.colorValue);
			}
			else if ((AmbientMode)ambientModeProperty.intValue == AmbientMode.Flat)
			{
				skyColorProperty.colorValue = EditorGUILayout.ColorField(skyColorProperty.displayName, skyColorProperty.colorValue);
			}

			EditorGUI.indentLevel -= 1;
			_serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck())
			{
				Environment.ApplyAmbientLighting((AmbientData)ambientDataProperty.managedReferenceValue);
				Environment.ApplySkybox((Material)skyboxMaterialProperty.objectReferenceValue);
			}
		}

		private void DrawHeader(Rect rect)
		{
			EditorGUI.LabelField(rect, "Spawn Points");
		}

		private void AddItem(ReorderableList reorderableList)
		{
			reorderableList.serializedProperty.arraySize += 1;
			var cameraTransform = ((SceneView)SceneView.sceneViews[0]).camera.transform;
			var spawnPointPosition = Vector3Int.FloorToInt(cameraTransform.position + cameraTransform.forward * 5);
			reorderableList.serializedProperty.GetArrayElementAtIndex(reorderableList.index).objectReferenceValue =
				null;

			var spawnPointsProperty = _serializedObject.FindProperty(GetBackingField(nameof(MapConfigure.SpawnPoints)));
			spawnPointsProperty.arraySize += 1;
		}

		private void RemoveItem(ReorderableList reorderableList)
		{
			DestroyImmediate(reorderableList.serializedProperty.GetArrayElementAtIndex(reorderableList.index)
				.objectReferenceValue);

			reorderableList.serializedProperty.DeleteArrayElementAtIndex(reorderableList.index);
			var spawnPointsProperty = _serializedObject.FindProperty(GetBackingField(nameof(MapConfigure.SpawnPoints)));
			spawnPointsProperty.DeleteArrayElementAtIndex(spawnPointsProperty.arraySize - 1);
		}

		private void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
		{
			var element = _spawnPoints.serializedProperty.GetArrayElementAtIndex(index);
			EditorGUI.ObjectField(rect, element.displayName, element.objectReferenceValue, typeof(GameObject),
				false);
		}

		private string GetBackingField(string fieldName)
		{
			return $"<{fieldName}>k__BackingField";
		}
	}
}