using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
namespace Angry_Girls
{
    [CustomEditor(typeof(DynamicFactoryExtention))]
    public class DynamicFactoryExtentionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // �������� ������ �� ������ CodeGenerator
            DynamicFactoryExtention generator = (DynamicFactoryExtention)target;

            // ������� ���� �������
            DrawDefaultInspector();

            // ������� ������ "Generate Code"
            if (GUILayout.Button("Generate Code"))
            {
                generator.GenerateCode();
            }
        }
    }
}
#endif