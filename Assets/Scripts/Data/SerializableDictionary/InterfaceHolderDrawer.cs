#if UNITY_EDITOR && !ODIN_INSPECTOR
using UnityEditor;
using UnityEngine;

namespace Data.SerializableDictionary
{
	[CustomPropertyDrawer(typeof(InterfaceHolder<>))]
	public class InterfaceHolderDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			SerializedProperty valueProperty = property.FindPropertyRelative("value");

			EditorGUI.BeginChangeCheck();
			var newValue = (MonoBehaviour)EditorGUI
				.ObjectField(position, label, valueProperty.objectReferenceValue,
					typeof(MonoBehaviour), true);

			if (EditorGUI.EndChangeCheck())
			{
				if (newValue == null || newValue.GetComponent(fieldInfo.FieldType.GenericTypeArguments[0]) != null)
				{
					valueProperty.objectReferenceValue = newValue;
				}
				else
				{
					Debug.LogWarning($"Assigned object must implement interface {fieldInfo.FieldType.GenericTypeArguments[0].Name}");
				}
			}

			EditorGUI.EndProperty();
		}
	}
}
#endif
