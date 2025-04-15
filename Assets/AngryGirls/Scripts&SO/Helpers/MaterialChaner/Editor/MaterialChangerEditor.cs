using UnityEngine;
using UnityEditor;

namespace Angry_Girls
{
    [CustomEditor(typeof(MaterialChanger))]
    public class MaterialChangerEditor : Editor
    {
        SerializedProperty material;
        SerializedProperty CurrentObjects;
        SerializedProperty CurrentMaterials;
        SerializedProperty NewMaterials;

        private void OnEnable()
        {
            material = serializedObject.FindProperty("material");
            CurrentObjects = serializedObject.FindProperty("CurrentObjects");
            CurrentMaterials = serializedObject.FindProperty("CurrentMaterials");
            NewMaterials = serializedObject.FindProperty("NewMaterials");
        }

        public override void OnInspectorGUI()
        {
            //DrawDefaultInspector();
            serializedObject.Update();

            MaterialChanger changer = (MaterialChanger)target;

            EditorGUILayout.Space();

            GUILayout.BeginVertical("box");

            EditorGUILayout.Space();



            if (GUILayout.Button("Identify Current Materials"))
            {
                if (CurrentObjects.arraySize != 0 || NewMaterials.arraySize != 0)
                {
                    CurrentObjects.ClearArray();
                    CurrentMaterials.ClearArray();
                    NewMaterials.ClearArray();
                }
                else
                {
                    changer.IdentifyMaterials();
                }
            }

            EditorGUILayout.Space();

            GUILayout.BeginHorizontal("box");

            GUILayout.BeginVertical("box");
            foreach (SerializedProperty s in CurrentObjects)
            {
                EditorGUILayout.PropertyField(s, new GUIContent(""));
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            foreach (SerializedProperty s in CurrentMaterials)
            {
                EditorGUILayout.PropertyField(s, new GUIContent(""));
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            foreach (SerializedProperty s in NewMaterials)
            {
                EditorGUILayout.PropertyField(s, new GUIContent(""));
            }
            GUILayout.EndVertical();


            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (GUILayout.Button("Switch Materials"))
            {
                if (CurrentObjects.arraySize != NewMaterials.arraySize)
                {
                    CurrentObjects.ClearArray();
                    CurrentMaterials.ClearArray();
                    NewMaterials.ClearArray();
                }
                else if (CurrentObjects.arraySize == 0)
                {
                    Debug.Log("List is empty");
                }
                else
                {
                    changer.ChangeComplexMaterial();
                }
            }

            EditorGUILayout.Space();

            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
}