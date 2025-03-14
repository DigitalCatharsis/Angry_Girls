using UnityEditor;
using UnityEngine;

namespace Angry_Girls
{
#if UNITY_EDITOR
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // ��������� �������� ���� GUI
            var previousGUIState = GUI.enabled;

            // ��������� �������������� ����
            GUI.enabled = false;

            // ������������ ����
            EditorGUI.PropertyField(position, property, label);

            // ��������������� �������� ��������� GUI
            GUI.enabled = previousGUIState;
        }
    }
    [CustomPropertyDrawer(typeof(ShowOnlyAttribute))]
#endif
    public class ShowOnlyAttribute : PropertyAttribute
    {
    }
}
