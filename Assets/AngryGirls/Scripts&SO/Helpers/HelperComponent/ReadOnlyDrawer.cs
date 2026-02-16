using UnityEditor;
using UnityEngine;

namespace Angry_Girls
{
#if UNITY_EDITOR
    /// <summary>
    /// Property drawer for read-only fields in Unity Inspector
    /// </summary>
    public class ReadOnlyDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Save the original GUI color
            var previousGUIState = GUI.enabled;

            // Disable field editing
            GUI.enabled = false;
            // Render the field
            EditorGUI.PropertyField(position, property, label);

            // Restore the original state of the GUI
            GUI.enabled = previousGUIState;
        }
    }
    [CustomPropertyDrawer(typeof(ShowOnlyAttribute))]
#endif
    public class ShowOnlyAttribute : PropertyAttribute
    {
    }
}
