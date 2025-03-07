#if UNITY_EDITOR
using Angry_Girls;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ShowOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Сохраняем исходный цвет GUI
        var previousGUIState = GUI.enabled;

        // Отключаем редактирование поля
        GUI.enabled = false;

        // Отрисовываем поле
        EditorGUI.PropertyField(position, property, label);

        // Восстанавливаем исходное состояние GUI
        GUI.enabled = previousGUIState;
    }
}
#endif

namespace Angry_Girls
{
    public class ShowOnlyAttribute : PropertyAttribute
    {
    }
}
