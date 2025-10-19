/*// THIS SHOULD BE PUT IN YOUR ASSETS/EDITOR FOLDER

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Data;
using GamePlay.Core;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityObject = UnityEngine.Object;

[HelpURL("https://forum.unity.com/threads/finally-a-serializable-dictionary-for-unity-extracted-from-system-collections-generic.335797/page-2")]
public abstract class DictionaryDrawer<TK, TV> : PropertyDrawer
{
    private SerializableDictionary<TK, TV> _Dictionary;
    private bool _Foldout;
    private const float kButtonWidth = 22f;
    private const int kMargin = 3;

    static readonly GUIContent iconToolbarMinus = EditorGUIUtility.IconContent("Toolbar Minus", "Remove selection from list");
    static readonly GUIContent iconToolbarPlus = EditorGUIUtility.IconContent("Toolbar Plus", "Add to list");
    static readonly GUIStyle preButton = "RL FooterButton";
    static readonly GUIStyle boxBackground = "RL Background";

    private bool _changed;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        CheckInitialize(property, label);
        if (!_Foldout)
            return 17f + kMargin * 2;

        int count = _Dictionary.Count;
        if (count == 0)
            return 17f + 16f + kMargin * 2;

        var type = typeof(TV);
        float contentHeight = 17f * count; // строка на каждый ключ

        if (typeof(IList).IsAssignableFrom(type))
        {
            // Оценка: ~40px на заголовок списка + ~20px на элемент
            contentHeight += count * 40f;
        }
        else if (!(_Fields.TryGetValue(type, out _) || type.IsEnum || typeof(UnityObject).IsAssignableFrom(type)))
        {
            var propCount = type
                .GetProperties()
                .Count(p => p.CanRead && p.GetIndexParameters().Length == 0);
            contentHeight += 17f * propCount * count;
        }

        return 17f + contentHeight + kMargin * 2;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        CheckInitialize(property, label);

        var backgroundRect = position;
        backgroundRect.xMin -= 7;
        backgroundRect.height += kMargin;
        if (Event.current.type == EventType.Repaint)
            boxBackground.Draw(backgroundRect, false, false, false, false);

        position.y += kMargin;
        position.height = 17f;

        var foldoutRect = position;
        foldoutRect.width -= 2 * kButtonWidth;
        EditorGUI.BeginChangeCheck();
        _Foldout = EditorGUI.Foldout(foldoutRect, _Foldout, label, true);
        if (EditorGUI.EndChangeCheck())
            EditorPrefs.SetBool(label.text, _Foldout);

        position.xMin += kMargin;
        position.xMax -= kMargin;

        var buttonRect = position;
        buttonRect.xMin = position.xMax - kButtonWidth;

        if (GUI.Button(buttonRect, iconToolbarMinus, preButton))
        {
            ClearDictionary();
        }

        buttonRect.x -= kButtonWidth - 1;

        if (GUI.Button(buttonRect, iconToolbarPlus, preButton))
        {
            AddNewItem();
        }

        if (!_Foldout)
            return;

        var labelRect = position;
        labelRect.y += 16;
        if (_Dictionary.Count == 0)
            GUI.Label(labelRect, "This dictionary doesn't have any items. Click + to add one!");

        foreach (var item in _Dictionary.ToList()) // ToList() чтобы избежать модификации во время итерации
        {
            var key = item.Key;
            var value = item.Value;

            position.y += 17f;

            var keyRect = position;
            keyRect.width /= 2;
            keyRect.width -= 4;
            EditorGUI.BeginChangeCheck();
            var newKey = DoField(keyRect, typeof(TK), key);

            if (EditorGUI.EndChangeCheck())
            {
                try
                {
                    _changed = true;
                    _Dictionary.Remove(key);
                    _Dictionary.Add(newKey, value);
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
                break;
            }

            var removeRect = position;
            removeRect.xMin = removeRect.xMax - kButtonWidth;
            if (GUI.Button(removeRect, iconToolbarMinus, preButton))
            {
                RemoveItem(key);
                break;
            }

            if (IsSupportedValueType())
            {
                var valueRect = position;
                valueRect.xMin = keyRect.xMax;
                valueRect.xMax = position.xMax - kButtonWidth;
                EditorGUI.BeginChangeCheck();
                value = DoField(valueRect, typeof(TV), value);
                position.y = valueRect.y;
                if (EditorGUI.EndChangeCheck())
                {
                    _changed = true;
                    _Dictionary[key] = value;
                    break;
                }
            }
            else
            {
                Type tv = typeof(TV);
                if (typeof(IList).IsAssignableFrom(tv))
                {
                    var list = (IList)value;
                    Type elementType = tv.IsGenericType && tv.GetGenericTypeDefinition() == typeof(List<>)
                        ? tv.GetGenericArguments()[0]
                        : typeof(object);

                    var reorderableList = new ReorderableList(list, elementType, true, true, true, true);
                    reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
                    {
                        var elem = list[index];
                        var newElem = DoField(rect, elementType, elem);
                        if (!Equals(elem, newElem))
                        {
                            list[index] = newElem;
                            _changed = true;
                        }
                    };

                    var valueRect = position;
                    valueRect.xMin = keyRect.xMax;
                    valueRect.xMax = position.xMax - kButtonWidth;

                    float listHeight = reorderableList.GetHeight();
                    position.y += listHeight;

                    reorderableList.DoList(valueRect);
                }
                else
                {
                    DrawValueProperties(position, ref value);
                    _Dictionary[key] = value;
                }
            }
        }

        if (_changed)
        {
            EditorUtility.SetDirty(property.serializedObject.targetObject);
            _changed = false;
        }
    }

    private void DrawValueProperties(Rect position, ref TV value)
    {
        var properties = typeof(TV).GetProperties()
            .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0)
            .ToArray();

        foreach (PropertyInfo property in properties)
        {
            position.y += 17;
            var currentValue = property.GetValue(value);
            var newValue = DoField(position, property.PropertyType, currentValue);
            if (!Equals(currentValue, newValue))
            {
                property.SetValue(value, newValue);
                _changed = true;
            }
        }
    }

    private void RemoveItem(TK key)
    {
        _Dictionary.Remove(key);
        _changed = true;
    }

    private void CheckInitialize(SerializedProperty property, GUIContent label)
    {
        if (_Dictionary == null)
        {
            var target = property.serializedObject.targetObject;
            _Dictionary = fieldInfo.GetValue(target) as SerializableDictionary<TK, TV>;
            if (_Dictionary == null)
            {
                _Dictionary = new SerializableDictionary<TK, TV>();
                fieldInfo.SetValue(target, _Dictionary);
            }

            _Foldout = EditorPrefs.GetBool(label.text, false);
        }
    }

    private static readonly Dictionary<Type, Func<Rect, object, object>> _Fields =
        new Dictionary<Type, Func<Rect, object, object>>()
        {
            { typeof(int), (rect, value) => EditorGUI.IntField(rect, (int)value) },
            { typeof(float), (rect, value) => EditorGUI.FloatField(rect, (float)value) },
            { typeof(string), (rect, value) => EditorGUI.TextField(rect, (string)value) },
            { typeof(bool), (rect, value) => EditorGUI.Toggle(rect, (bool)value) },
            { typeof(Vector2), (rect, value) => EditorGUI.Vector2Field(rect, GUIContent.none, (Vector2)value) },
            { typeof(Vector3), (rect, value) => EditorGUI.Vector3Field(rect, GUIContent.none, (Vector3)value) },
            { typeof(Bounds), (rect, value) => EditorGUI.BoundsField(rect, (Bounds)value) },
            { typeof(Rect), (rect, value) => EditorGUI.RectField(rect, (Rect)value) },
        };

    private static T DoField<T>(Rect rect, Type type, T value)
    {
        if (_Fields.TryGetValue(type, out var field))
            return (T)field(rect, value);

        if (type.IsEnum)
            return (T)(object)EditorGUI.EnumPopup(rect, (Enum)(object)value);

        if (typeof(UnityObject).IsAssignableFrom(type))
            return (T)(object)EditorGUI.ObjectField(rect, (UnityObject)(object)value, type, true);

        // Fallback: не редактируемый тип
        EditorGUI.LabelField(rect, value?.ToString() ?? "null");
        return value;
    }

    private bool IsSupportedValueType()
    {
        Type type = typeof(TV);
        if (_Fields.ContainsKey(type))
            return true;
        if (type.IsEnum)
            return true;
        if (typeof(UnityObject).IsAssignableFrom(type))
            return true;
        return false;
    }

    private void ClearDictionary()
    {
        _Dictionary.Clear();
        _changed = true;
    }

    private void AddNewItem()
    {
        _changed = true;

        TK key = default;
        TV value = default;

        if (typeof(TK).IsEnum)
        {
            var values = Enum.GetValues(typeof(TK));
            if (values.Length > 0)
                key = (TK)values.GetValue(0);
        }
        else if (typeof(TK) == typeof(string))
        {
            key = (TK)(object)"";
        }

        if (typeof(IList).IsAssignableFrom(typeof(TV)))
        {
            Type elementType;
            if (typeof(TV).IsGenericType && typeof(TV).GetGenericTypeDefinition() == typeof(List<>))
            {
                elementType = typeof(TV).GetGenericArguments()[0];
            }
            else if (typeof(TV).IsArray)
            {
                elementType = typeof(TV).GetElementType();
            }
            else
            {
                elementType = typeof(object);
            }

            Type listType = typeof(List<>).MakeGenericType(elementType);
            var emptyList = Activator.CreateInstance(listType);
            value = (TV)emptyList;
        }

        try
        {
            _Dictionary.Add(key, value);
        }
        catch (Exception e)
        {
            Debug.Log($"Failed to add new item: {e.Message}");
        }
    }
}

// --- Твои конкретные драверы ---

[CustomPropertyDrawer(typeof(SpriteByItemTypeDict))]
public class SpriteByItemTypeDictDrawer : DictionaryDrawer<ItemType, Sprite> { }

[CustomPropertyDrawer(typeof(ConfigureByItemTypeDict))]
public class ConfigureByItemTypeDictDrawer : DictionaryDrawer<ItemType, InventoryItemConfigure> { }

[CustomPropertyDrawer(typeof(CharacteristicByGameClassDict))]
public class CharacteristicByGameClassDictDrawer : DictionaryDrawer<GameClass, Characteristics> { }

[CustomPropertyDrawer(typeof(ItemsByGameClassDict))]
public class ItemsByGameClassDictDrawer : DictionaryDrawer<GameClass, List<ItemType>> { }*/