using Data;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(MapConfigure))]
    public class MapConfigureEditor : UnityEditor.Editor
    {
        private SerializedProperty _spawnPointList;

        private void OnEnable()
        {
            _spawnPointList = serializedObject.FindProperty("spawnPoints");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var iterator = serializedObject.GetIterator();
            for (var enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                if (iterator.name == "spawnPoints")
                {
                    DrawSpawnPointList();
                    continue;
                }

                using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
                    EditorGUILayout.PropertyField(iterator, true);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSpawnPointList()
        {
            EditorGUILayout.PropertyField(_spawnPointList, false);
            EditorGUI.indentLevel += 1;
            GUI.enabled = false;
            for (var i = 0; i < _spawnPointList.arraySize; i++)
            {
                EditorGUILayout.PropertyField(_spawnPointList.GetArrayElementAtIndex(i));
            }

            GUI.enabled = true;
            EditorGUI.indentLevel -= 1;
        }
    }
}