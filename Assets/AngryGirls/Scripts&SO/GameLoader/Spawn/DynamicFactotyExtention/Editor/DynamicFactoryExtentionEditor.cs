using UnityEditor;
using UnityEngine;

namespace Angry_Girls
{
    [CustomEditor(typeof(DynamicFactoryExtention))]
    public class DynamicFactoryExtentionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Получаем ссылку на скрипт CodeGenerator
            DynamicFactoryExtention generator = (DynamicFactoryExtention)target;

            // Выводим поля скрипта
            DrawDefaultInspector();

            // Создаем кнопку "Generate Code"
            if (GUILayout.Button("Generate Code"))
            {
                generator.GenerateCode();
            }
        }
    }
}