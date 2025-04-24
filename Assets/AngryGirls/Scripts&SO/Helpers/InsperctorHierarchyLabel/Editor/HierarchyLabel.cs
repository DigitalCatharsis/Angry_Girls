using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
/// <summary>
/// Check out http://diegogiacomelli.com.br/unitytips-hierarchy-window-group-header/
/// </summary>
namespace Angry_Girls
{
    [InitializeOnLoad]
    public class HierarchyLabel : MonoBehaviour
    {
        static HierarchyLabel()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
        }

        static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            if (obj != null && obj.name.StartsWith("___", System.StringComparison.Ordinal))    //Every gameobject which starts with __- gonna be painted and have a shadow label
            {
                EditorGUI.DrawRect(selectionRect, Color.grey);
                EditorGUI.DropShadowLabel(selectionRect, obj.name.Replace("_", "").ToString());
            }

            HighLightObj(obj, selectionRect);
        }

        static void HighLightObj(GameObject obj, Rect selectionRect)
        {
            if (obj != null && obj.name.StartsWith("__R", System.StringComparison.Ordinal))    //Every gameobject which starts with __- gonna be painted and have a shadow label
            {
                EditorGUI.DrawRect(selectionRect, Color.red);
                EditorGUI.DropShadowLabel(selectionRect, obj.name.Replace("__R", "").ToString());
            }
            else if (obj != null && obj.name.StartsWith("__G", System.StringComparison.Ordinal))    //Every gameobject which starts with __- gonna be painted and have a shadow label
            {
                EditorGUI.DrawRect(selectionRect, Color.green);
                EditorGUI.DropShadowLabel(selectionRect, obj.name.Replace("__G", "").ToString());
            }
            else if (obj != null && obj.name.StartsWith("__B", System.StringComparison.Ordinal))    //Every gameobject which starts with __- gonna be painted and have a shadow label
            {
                EditorGUI.DrawRect(selectionRect, Color.blue);
                EditorGUI.DropShadowLabel(selectionRect, obj.name.Replace("__B", "").ToString());
            }
            else if (obj != null && obj.name.StartsWith("__Y", System.StringComparison.Ordinal))    //Every gameobject which starts with __- gonna be painted and have a shadow label
            {
                EditorGUI.DrawRect(selectionRect, Color.yellow);
                EditorGUI.DropShadowLabel(selectionRect, obj.name.Replace("__Y", "").ToString());
            }
            else if (obj != null && obj.name.StartsWith("__M", System.StringComparison.Ordinal))    //Every gameobject which starts with __- gonna be painted and have a shadow label
            {
                EditorGUI.DrawRect(selectionRect, Color.magenta);
                EditorGUI.DropShadowLabel(selectionRect, obj.name.Replace("__M", "").ToString());
            }
            else if (obj != null && obj.name.StartsWith("__C", System.StringComparison.Ordinal))    //Every gameobject which starts with __- gonna be painted and have a shadow label
            {
                EditorGUI.DrawRect(selectionRect, Color.cyan);
                EditorGUI.DropShadowLabel(selectionRect, obj.name.Replace("__C", "").ToString());
            }
            else if (obj != null && obj.name.StartsWith($"{typeof(HierarchyLabel).Namespace}.", System.StringComparison.Ordinal))    //Every gameobject which starts with __- gonna be painted and have a shadow label
            {
                EditorGUI.DrawRect(selectionRect, Color.black);
                EditorGUI.DropShadowLabel(selectionRect, obj.name.ToString());
            }          
        }
    }
}
#endif