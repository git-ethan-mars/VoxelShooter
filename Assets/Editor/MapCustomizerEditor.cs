using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

[CustomEditor(typeof(MapCustomizer))]
public class MapCustomizerEditor : UnityEditor.Editor
{
    private SerializedObject _configure;
    private SerializedProperty _lightProperty;
    private SerializedProperty _skyboxProperty;
    private SerializedProperty _configureProperty;
    private SerializedProperty _waterColorProperty;
    private SerializedProperty _innerColorProperty;
    private SerializedProperty _spawnPointsProperty;
    private SerializedProperty _skyColorProperty;
    private SerializedProperty _ambientModeProperty;
    private SerializedProperty _equatorColorProperty;
    private SerializedProperty _groundColorProperty;
    private SerializedProperty _ambientIntensityProperty;
    private SerializedProperty _isFogActivatedProperty;
    private SerializedProperty _fogModeProperty;
    private SerializedProperty _fogColorProperty;
    private SerializedProperty _fogStartDistanceProperty;
    private SerializedProperty _fogEndDistanceProperty;
    private SerializedProperty _fogDensityProperty;
    private MapCustomizer _mapEditorScript;
    private ReorderableList _spawnPoints;
    private readonly string[] _ambientModes = {"Skybox", "Gradient", "Color"};

    private void OnEnable()
    {
        _mapEditorScript = (MapCustomizer) target;
        _configure = new SerializedObject(_mapEditorScript.mapConfigure);
        _spawnPoints = new ReorderableList(_mapEditorScript.spawnPoints, typeof(GameObject), false, true, true, true)
        {
            drawHeaderCallback = DrawHeader,
            drawElementCallback = DrawListItems,
            onAddCallback = AddItem,
            onRemoveCallback = RemoveItem
        };
        _lightProperty = serializedObject.FindProperty("lightSource");
        _skyboxProperty = _configure.FindProperty("skyboxMaterial");
        _configureProperty = serializedObject.FindProperty("mapConfigure");
        _waterColorProperty = serializedObject.FindProperty("waterColor");
        _innerColorProperty = serializedObject.FindProperty("innerColor");
        _spawnPointsProperty = serializedObject.FindProperty("spawnPoints");
        _ambientModeProperty = serializedObject.FindProperty("ambientMode");
        _skyColorProperty = serializedObject.FindProperty("skyColor");
        _equatorColorProperty = serializedObject.FindProperty("equatorColor");
        _groundColorProperty = serializedObject.FindProperty("groundColor");
        _ambientIntensityProperty = serializedObject.FindProperty("ambientIntensity");
        _isFogActivatedProperty = serializedObject.FindProperty("isFogActivated");
        _fogModeProperty = serializedObject.FindProperty("fogMode");
        _fogColorProperty = serializedObject.FindProperty("fogColor");
        _fogStartDistanceProperty = serializedObject.FindProperty("fogStartDistance");
        _fogEndDistanceProperty = serializedObject.FindProperty("fogEndDistance");
        _fogDensityProperty = serializedObject.FindProperty("fogDensity");
    }

    private void DrawHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, _spawnPointsProperty.displayName);
    }

    private void AddItem(ReorderableList reorderableList)
    {
        var spawnPoint = _mapEditorScript.CreateSpawnPoint(Vector3Int.zero);
        reorderableList.list.Add(spawnPoint);
    }

    private void RemoveItem(ReorderableList reorderableList)
    {
        var spawnPoint = reorderableList.list[reorderableList.index] as GameObject;
        DestroyImmediate(spawnPoint);
        reorderableList.list.RemoveAt(reorderableList.index);
        serializedObject.Update();
    }

    private void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
    {
        serializedObject.ApplyModifiedProperties();
        serializedObject.Update();
        var element = _spawnPointsProperty.GetArrayElementAtIndex(index);
        EditorGUI.ObjectField(rect, element.displayName, element.objectReferenceValue, typeof(GameObject),
            false);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(_configureProperty);

        if (_mapEditorScript.mapConfigure == null)
        {
            EditorGUILayout.HelpBox("Create/choose map configure file", MessageType.Error);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        if (GUILayout.Button("Generate map"))
        {
            _mapEditorScript.GenerateMap();
        }

        if (!_mapEditorScript.IsMapGenerated)
        {
            serializedObject.ApplyModifiedProperties();
            return;
        }

        EditorGUILayout.LabelField("Color blocks");
        EditorGUI.indentLevel += 1;
        EditorGUILayout.PropertyField(_waterColorProperty);
        EditorGUILayout.PropertyField(_innerColorProperty);
        EditorGUI.indentLevel -= 1;

        EditorGUILayout.PropertyField(_lightProperty);
        if (_mapEditorScript.lightSource != null)
        {
            if (GUILayout.Button("Save light settings"))
            {
                _mapEditorScript.SaveLighting();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Choose light source in the scene", MessageType.Warning);
        }

        EditorGUILayout.LabelField("Ambient settings");
        EditorGUI.indentLevel += 1;
        _ambientModeProperty.intValue =
            EditorGUILayout.IntPopup("Source", _ambientModeProperty.intValue, _ambientModes, new[] {0, 1, 3});
        _mapEditorScript.mapConfigure.ambientMode = (AmbientMode) _ambientModeProperty.intValue;

        switch ((AmbientMode) _ambientModeProperty.intValue)
        {
            case AmbientMode.Skybox:
                if (_skyboxProperty.objectReferenceValue == null)
                {
                    _skyColorProperty.colorValue = EditorGUILayout.ColorField(
                        new GUIContent(_skyColorProperty.displayName), _skyColorProperty.colorValue, true,
                        false, true);
                    _mapEditorScript.skyColor = _skyColorProperty.colorValue;
                }
                else
                {
                    _ambientIntensityProperty.floatValue =
                        EditorGUILayout.Slider("Intensity Multiplier",
                            _ambientIntensityProperty.floatValue, 0.0F, 8.0F);
                    _mapEditorScript.ambientIntensity = _ambientIntensityProperty.floatValue;
                }

                break;
            case AmbientMode.Trilight:
            {
                _skyColorProperty.colorValue = EditorGUILayout.ColorField(
                    new GUIContent(_skyColorProperty.displayName),
                    _skyColorProperty.colorValue, true,
                    false, true);
                _mapEditorScript.skyColor = _skyColorProperty.colorValue;
                _mapEditorScript.mapConfigure.skyColor = _skyColorProperty.colorValue;
                _equatorColorProperty.colorValue = EditorGUILayout.ColorField(
                    new GUIContent(_equatorColorProperty.displayName),
                    _equatorColorProperty.colorValue, true,
                    false, true);
                _mapEditorScript.equatorColor = _equatorColorProperty.colorValue;
                _mapEditorScript.mapConfigure.equatorColor = _equatorColorProperty.colorValue;
                _groundColorProperty.colorValue = EditorGUILayout.ColorField(
                    new GUIContent(_groundColorProperty.displayName),
                    _groundColorProperty.colorValue, true,
                    false, true);
                _mapEditorScript.groundColor = _groundColorProperty.colorValue;
                _mapEditorScript.mapConfigure.groundColor = _groundColorProperty.colorValue;
            }
                break;
            case AmbientMode.Flat:
            {
                _skyColorProperty.colorValue = EditorGUILayout.ColorField(
                    new GUIContent(_skyColorProperty.displayName), _skyColorProperty.colorValue, true,
                    false, true);
                _mapEditorScript.skyColor = _skyColorProperty.colorValue;
                _mapEditorScript.mapConfigure.skyColor = _skyColorProperty.colorValue;
            }
                break;
        }

        EditorGUI.indentLevel -= 1;

        EditorGUILayout.LabelField("Fog");
        EditorGUI.indentLevel += 1;
        EditorGUILayout.PropertyField(_isFogActivatedProperty);
        _mapEditorScript.mapConfigure.isFogActivated = _isFogActivatedProperty.boolValue;
        if (_isFogActivatedProperty.boolValue)
        {
            EditorGUILayout.PropertyField(_fogColorProperty);
            _mapEditorScript.mapConfigure.fogColor = _fogColorProperty.colorValue;
            EditorGUILayout.PropertyField(_fogModeProperty);
            _mapEditorScript.mapConfigure.fogMode = (FogMode) _fogModeProperty.intValue;
            if ((FogMode) _fogModeProperty.intValue != FogMode.Linear)
            {
                EditorGUILayout.PropertyField(_fogDensityProperty);
                _mapEditorScript.mapConfigure.fogDensity = _fogDensityProperty.floatValue;
            }
            else
            {
                EditorGUILayout.PropertyField(_fogStartDistanceProperty);
                _mapEditorScript.mapConfigure.fogStartDistance = _fogStartDistanceProperty.floatValue;
                EditorGUILayout.PropertyField(_fogEndDistanceProperty);
                _mapEditorScript.mapConfigure.fogEndDistance = _fogEndDistanceProperty.floatValue;
            }
        }

        EditorGUI.indentLevel -= 1;

        EditorGUILayout.PropertyField(_skyboxProperty);

        if (_mapEditorScript.skyboxMaterial != null)
        {
            if (GUILayout.Button("Show skybox"))
            {
                RenderSettings.skybox = _mapEditorScript.skyboxMaterial;
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Choose skybox material", MessageType.Warning);
        }

        _spawnPoints.DoLayoutList();

        if (GUILayout.Button("Save SO"))
        {
            EditorUtility.SetDirty(_mapEditorScript.mapConfigure);
            AssetDatabase.SaveAssets();
        }

        serializedObject.ApplyModifiedProperties();
    }
}