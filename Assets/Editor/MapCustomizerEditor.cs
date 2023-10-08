using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

[CustomEditor(typeof(MapCustomizer))]
public class MapCustomizerEditor : UnityEditor.Editor
{
    private SerializedProperty _lightProperty;
    private MapCustomizer _mapEditorScript;
    private ReorderableList _spawnPoints;
    private readonly string[] _ambientModes = {"Skybox", "Gradient", "Color"};
    private readonly string[] _fogModes = {"Linear", "Exponential", "Exponential squared"};
    private SerializedObject _configure;

    private void OnEnable()
    {
        _mapEditorScript = (MapCustomizer) target;
        _spawnPoints = new ReorderableList(serializedObject, serializedObject.FindProperty("spawnPoints"), false, true,
            true, true)
        {
            drawHeaderCallback = DrawHeader,
            drawElementCallback = DrawListItems,
            onAddCallback = AddItem,
            onRemoveCallback = RemoveItem
        };
        _lightProperty = serializedObject.FindProperty("lightSource");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawGUI();
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawGUI()
    {
        var configureProperty = serializedObject.FindProperty("mapConfigure");

        EditorGUILayout.PropertyField(configureProperty);
        if (configureProperty.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Create/choose map configure file", MessageType.Error);
            return;
        }

        _configure = new SerializedObject(configureProperty.objectReferenceValue);
        var imageProperty = _configure.FindProperty("image");
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.ObjectField(imageProperty, new GUIContent(imageProperty.displayName));
        if (EditorGUI.EndChangeCheck())
        {
            _configure.ApplyModifiedProperties();
        }

        if (GUILayout.Button("Generate map"))
        {
            _mapEditorScript.GenerateMap();
        }

        if (!_mapEditorScript.IsMapGenerated)
        {
            return;
        }

        EditorGUILayout.LabelField("Color blocks");
        EditorGUI.indentLevel += 1;
        var waterColorProperty = _configure.FindProperty("waterColor");
        var innerColorProperty = _configure.FindProperty("innerColor");
        var newWaterColor = EditorGUILayout.ColorField(new GUIContent(waterColorProperty.displayName),
            waterColorProperty.colorValue);
        var newInnerColor = EditorGUILayout.ColorField(innerColorProperty.displayName,
            innerColorProperty.colorValue);
        waterColorProperty.colorValue = newWaterColor;
        innerColorProperty.colorValue = newInnerColor;

        EditorGUI.indentLevel -= 1;
        EditorGUILayout.PropertyField(_lightProperty);
        if (_lightProperty.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Choose light source in the scene", MessageType.Warning);
        }

        DrawAmbientProperties();
        DrawFogProperties();
        _spawnPoints.DoLayoutList();
        _configure.ApplyModifiedProperties();
        if (EditorUtility.IsDirty(_mapEditorScript.mapConfigure) && GUILayout.Button("Save configure"))
        {
            AssetDatabase.SaveAssets();
        }
    }

    private void DrawFogProperties()
    {
        var isFogActivatedProperty = _configure.FindProperty("isFogActivated");
        EditorGUI.BeginChangeCheck();
        var isFogActivated =
            EditorGUILayout.Toggle("Fog", isFogActivatedProperty.boolValue);
        isFogActivatedProperty.boolValue = isFogActivated;
        EditorGUI.indentLevel += 1;
        if (isFogActivatedProperty.boolValue)
        {
            var fogColorProperty = _configure.FindProperty("fogColor");
            var newFogColor = EditorGUILayout.ColorField(fogColorProperty.displayName, fogColorProperty.colorValue);
            fogColorProperty.colorValue = newFogColor;
            var fogModeProperty = _configure.FindProperty("fogMode");
            fogModeProperty.intValue = EditorGUILayout.IntPopup(fogModeProperty.displayName, fogModeProperty.intValue,
                _fogModes, new[] {1, 2, 3});
            if ((FogMode) fogModeProperty.intValue != FogMode.Linear)
            {
                var fogDensityProperty = _configure.FindProperty("fogDensity");
                var newFogDensity =
                    EditorGUILayout.FloatField(fogDensityProperty.displayName, fogDensityProperty.floatValue);
                fogDensityProperty.floatValue = newFogDensity;
            }
            else
            {
                var fogStartDistanceProperty = _configure.FindProperty("fogStartDistance");
                var fogEndDistanceProperty = _configure.FindProperty("fogEndDistance");
                var newFogStartDistance = EditorGUILayout.FloatField(fogStartDistanceProperty.displayName,
                    fogStartDistanceProperty.floatValue);
                var newFogEndDistance = EditorGUILayout.FloatField(fogEndDistanceProperty.displayName,
                    fogEndDistanceProperty.floatValue);
                fogStartDistanceProperty.floatValue = newFogStartDistance;
                fogEndDistanceProperty.floatValue = newFogEndDistance;
            }
        }

        EditorGUI.indentLevel -= 1;
        _configure.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
        {
            _mapEditorScript.ShowFog();
        }
    }

    private void DrawAmbientProperties()
    {
        EditorGUILayout.LabelField("Ambient settings");
        EditorGUI.BeginChangeCheck();
        EditorGUI.indentLevel += 1;
        var skyboxMaterialProperty = _configure.FindProperty("skyboxMaterial");
        EditorGUILayout.ObjectField(skyboxMaterialProperty, new GUIContent(skyboxMaterialProperty.displayName));
        var ambientModeProperty = _configure.FindProperty("ambientMode");
        ambientModeProperty.intValue =
            EditorGUILayout.IntPopup("Source", ambientModeProperty.intValue, _ambientModes, new[] {0, 1, 3});
        var skyColorProperty = _configure.FindProperty("skyColor");
        var equatorColorProperty = _configure.FindProperty("equatorColor");
        var groundColorProperty = _configure.FindProperty("groundColor");
        if ((AmbientMode) ambientModeProperty.intValue == AmbientMode.Skybox)
        {
            if (skyboxMaterialProperty.objectReferenceValue == null)
            {
                var newSkyColor = EditorGUILayout.ColorField(skyColorProperty.displayName, skyColorProperty.colorValue);
                skyColorProperty.colorValue = newSkyColor;
            }
            else
            {
                var ambientIntensityProperty = _configure.FindProperty("ambientIntensity");
                ambientIntensityProperty.floatValue =
                    EditorGUILayout.Slider("Intensity Multiplier",
                        ambientIntensityProperty.floatValue, 0.0F, 8.0F);
            }
        }
        else if ((AmbientMode) ambientModeProperty.intValue == AmbientMode.Trilight)
        {
            var newSkyColor = EditorGUILayout.ColorField(skyColorProperty.displayName, skyColorProperty.colorValue);
            var newEquatorColor =
                EditorGUILayout.ColorField(equatorColorProperty.displayName, equatorColorProperty.colorValue);
            var newGroundColor =
                EditorGUILayout.ColorField(groundColorProperty.displayName, groundColorProperty.colorValue);
            skyColorProperty.colorValue = newSkyColor;
            equatorColorProperty.colorValue = newEquatorColor;
            groundColorProperty.colorValue = newGroundColor;
        }
        else if ((AmbientMode) ambientModeProperty.intValue == AmbientMode.Flat)
        {
            var newSkyColor = EditorGUILayout.ColorField(skyColorProperty.displayName, skyColorProperty.colorValue);
            skyColorProperty.colorValue = newSkyColor;
        }

        EditorGUI.indentLevel -= 1;
        _configure.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
        {
            _mapEditorScript.ShowAmbientLighting();
        }
    }

    private void DrawHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, "Spawn points");
    }

    private void AddItem(ReorderableList reorderableList)
    {
        reorderableList.serializedProperty.arraySize += 1;
        reorderableList.serializedProperty.GetArrayElementAtIndex(reorderableList.index).objectReferenceValue =
            _mapEditorScript.CreateSpawnPoint(
                Vector3Int.FloorToInt(((SceneView) SceneView.sceneViews[0]).camera.transform.position));
        var spawnPointsProperty = _configure.FindProperty("spawnPoints");
        spawnPointsProperty.arraySize += 1;
    }

    private void RemoveItem(ReorderableList reorderableList)
    {
        DestroyImmediate(reorderableList.serializedProperty.GetArrayElementAtIndex(reorderableList.index)
            .objectReferenceValue);
        reorderableList.serializedProperty.DeleteArrayElementAtIndex(reorderableList.index);
        var spawnPointsProperty = _configure.FindProperty("spawnPoints");
        spawnPointsProperty.DeleteArrayElementAtIndex(spawnPointsProperty.arraySize - 1);
    }

    private void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
    {
        var element = _spawnPoints.serializedProperty.GetArrayElementAtIndex(index);
        EditorGUI.ObjectField(rect, element.displayName, element.objectReferenceValue, typeof(GameObject),
            false);
    }
}