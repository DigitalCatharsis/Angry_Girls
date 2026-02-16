#if (UNITY_EDITOR) 
using UnityEngine;
using UnityEditor;

namespace Angry_Girls
{
    /// <summary>
    /// Custom editor for FactoryExtention
    /// </summary>
    [CustomEditor(typeof(FactoryExtention))]
    public class FactoryExtentionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            FactoryExtention generator = (FactoryExtention)target;

            DrawDefaultInspector();

            if (GUILayout.Button("Generate Code"))
            {
                generator.GenerateCode();
            }
        }
    }
}

#endif