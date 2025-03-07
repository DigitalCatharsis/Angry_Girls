#if UNITY_EDITOR
using Angry_Girls;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ShowOnlyAttribute))]
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
#endif

namespace Angry_Girls
{
    public class ShowOnlyAttribute : PropertyAttribute
    {
    }
}
