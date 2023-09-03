using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapEditor))]
public class MapGeneratorEditor : UnityEditor.Editor
{
    private SerializedProperty _lightProperty;
    private SerializedProperty _skyboxProperty;
    private SerializedProperty _configureProperty;
    private MapEditor _mapEditorScript;

    private void OnEnable()
    {
        _lightProperty = serializedObject.FindProperty("lightSource");
        _skyboxProperty = serializedObject.FindProperty("skybox");
        _configureProperty = serializedObject.FindProperty("mapConfigure");
        _mapEditorScript = (MapEditor) target;
    }

    public override void OnInspectorGUI()
    {
        _mapEditorScript.mapConfigure = EditorGUILayout.ObjectField(_configureProperty.displayName,
            _configureProperty.objectReferenceValue, typeof(MapConfigure), true) as MapConfigure;
        if (_mapEditorScript.mapConfigure == null)
        {
            EditorGUILayout.HelpBox("Create/choose map configure file", MessageType.Error);
            return;
        }

        if (GUILayout.Button("Generate map"))
        {
            _mapEditorScript.GenerateMap();
        }


        _mapEditorScript.lightSource = EditorGUILayout.ObjectField(_lightProperty.displayName,
            _lightProperty.objectReferenceValue, typeof(Light),
            true) as Light;

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

        _mapEditorScript.skybox =
            EditorGUILayout.ObjectField(_skyboxProperty.displayName, _skyboxProperty.objectReferenceValue,
                typeof(Material), true) as Material;

        if (_mapEditorScript.skybox != null)
        {
            if (GUILayout.Button("Save skybox"))
            {
                _mapEditorScript.SaveSkybox();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Choose skybox material", MessageType.Warning);
        }
    }
}