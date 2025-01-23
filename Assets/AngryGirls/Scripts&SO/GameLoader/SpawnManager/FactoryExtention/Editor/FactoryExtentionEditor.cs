using UnityEngine;
using UnityEditor;

namespace Angry_Girls
{
    [CustomEditor(typeof(FactoryExtention))]
    public class FactoryExtentionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Получаем ссылку на скрипт CodeGenerator
            FactoryExtention generator = (FactoryExtention)target;

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