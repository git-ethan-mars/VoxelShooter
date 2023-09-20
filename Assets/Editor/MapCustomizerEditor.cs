using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(MapCustomizer))]
public class MapCustomizerEditor : UnityEditor.Editor
{
    private SerializedProperty _lightProperty;
    private SerializedProperty _skyboxProperty;
    private SerializedProperty _configureProperty;
    private SerializedProperty _waterColorProperty;
    private SerializedProperty _innerColorProperty;
    private SerializedProperty _spawnPointsProperty;
    private MapCustomizer _mapEditorScript;
    private ReorderableList _spawnPoints;

    private void OnEnable()
    {
        _lightProperty = serializedObject.FindProperty("lightSource");
        _skyboxProperty = serializedObject.FindProperty("skybox");
        _configureProperty = serializedObject.FindProperty("mapConfigure");
        _waterColorProperty = serializedObject.FindProperty("waterColor");
        _innerColorProperty = serializedObject.FindProperty("innerColor");
        _spawnPointsProperty = serializedObject.FindProperty("spawnPoints");
        _mapEditorScript = (MapCustomizer) target;
        _spawnPoints = new ReorderableList(_mapEditorScript.spawnPoints, typeof(GameObject), false, true, true, true)
        {
            drawHeaderCallback = DrawHeader,
            drawElementCallback = DrawListItems,
            onAddCallback = AddItem,
            onRemoveCallback = RemoveItem
        };
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

        EditorGUILayout.PropertyField(_waterColorProperty);
        EditorGUILayout.PropertyField(_innerColorProperty);
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

        EditorGUILayout.PropertyField(_skyboxProperty);

        if (_mapEditorScript.skybox != null)
        {
            if (GUILayout.Button("Show skybox"))
            {
                RenderSettings.skybox = _mapEditorScript.skybox;
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Choose skybox material", MessageType.Warning);
        }

        _spawnPoints.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }
}