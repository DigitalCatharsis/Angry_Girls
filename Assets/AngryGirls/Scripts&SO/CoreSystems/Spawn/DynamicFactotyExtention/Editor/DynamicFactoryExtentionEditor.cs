using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
namespace Angry_Girls
{
    /// <summary>
    /// Custom editor for DynamicFactoryExtention
    /// </summary>
    [CustomEditor(typeof(DynamicFactoryExtention))]
    public class DynamicFactoryExtentionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DynamicFactoryExtention generator = (DynamicFactoryExtention)target;

            DrawDefaultInspector();

            if (GUILayout.Button("Generate Code"))
            {
                generator.GenerateCode();
            }
        }
    }
}
#endif