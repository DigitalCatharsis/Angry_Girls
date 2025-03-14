using UnityEditor;
using UnityEngine;

namespace Angry_Girls
{
#if UNITY_EDITOR
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
    [CustomPropertyDrawer(typeof(ShowOnlyAttribute))]
#endif
    public class ShowOnlyAttribute : PropertyAttribute
    {
    }
}
