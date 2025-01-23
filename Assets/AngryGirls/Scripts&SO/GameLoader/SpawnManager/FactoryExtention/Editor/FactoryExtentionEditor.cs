using UnityEngine;
using UnityEditor;

namespace Angry_Girls
{
    [CustomEditor(typeof(FactoryExtention))]
    public class FactoryExtentionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // �������� ������ �� ������ CodeGenerator
            FactoryExtention generator = (FactoryExtention)target;

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