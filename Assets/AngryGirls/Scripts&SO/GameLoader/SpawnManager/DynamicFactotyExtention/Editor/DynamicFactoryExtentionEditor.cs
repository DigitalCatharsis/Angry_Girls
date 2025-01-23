using UnityEditor;
using UnityEngine;

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