using UnityEngine;
using UnityEditor;  //Для срани ниже
#if UNITY_EDITOR
namespace Angry_Girls
{
    [CustomEditor(typeof(LookAtPoint))] //The CustomEditor attribute informs Unity which component it should act as an editor for.
    [CanEditMultipleObjects]  //The CanEditMultipleObjects attribute tells Unity that you can select multiple objects with this editor and change them all at the same time.

    public class LookAtPointEditor : Editor  //Наследуемся от Editor
    {
        SerializedProperty lookAtPoint; //Дженерик свойства для ловли полей
        SerializedProperty objectToLook; //Дженерик свойства для ловли полей
        SerializedProperty rotateToPoint; //Дженерик свойства для ловли полей
        SerializedProperty traceColor; //Дженерик свойства для ловли полей
        SerializedProperty traceExistDuration; //Дженерик свойства для ловли полей

        void OnEnable()
        {
            
            lookAtPoint = serializedObject.FindProperty("lookAtPoint"); //Какое поле хватаем
            objectToLook = serializedObject.FindProperty("objectToLook"); //Какое поле хватаем
            rotateToPoint = serializedObject.FindProperty("rotateToPoint"); //Какое поле хватаем
            traceColor = serializedObject.FindProperty("traceColor"); //Какое поле хватаем
            traceExistDuration = serializedObject.FindProperty("traceExistDuration"); //Какое поле хватаем
        }

        public override void OnInspectorGUI()  //GUI инспектора
        {
            var tar = (target as LookAtPoint);
            tar.Update(); //Используем Update, описанный в классе tar

            serializedObject.Update(); //Тыкнуть поле, чтобы оно считало



            EditorGUILayout.PropertyField(lookAtPoint); //Отобразить обрабатываемое поле
            EditorGUILayout.PropertyField(objectToLook); //Отобразить обрабатываемое поле
            EditorGUILayout.PropertyField(rotateToPoint); //Отобразить обрабатываемое поле
            EditorGUILayout.PropertyField(traceColor); //Отобразить обрабатываемое поле
            EditorGUILayout.PropertyField(traceExistDuration); //Отобразить обрабатываемое поле

            if (lookAtPoint.vector3Value.y > (target as LookAtPoint).transform.position.y)
            {
                EditorGUILayout.LabelField("(Selected object is below lookAtPoint)"); //Создай TIP
            }

            if (lookAtPoint.vector3Value.y < (target as LookAtPoint).transform.position.y)
            {
                EditorGUILayout.LabelField("(Selected object is above lookAtPoint)");
            }

            if (lookAtPoint.vector3Value.y == (target as LookAtPoint).transform.position.y)
            {
                EditorGUILayout.LabelField("Selected object at the same \"y\" level as lookAtPoint");
            }

            EditorGUILayout.LabelField($"Vector range to point: {(lookAtPoint.vector3Value - (target as LookAtPoint).transform.position)}");
            EditorGUILayout.LabelField("This is working even if script is disabled!"); //Создай TIP
            serializedObject.ApplyModifiedProperties(); //Без этой строки не выйдет изменить значения

            if (GUILayout.Button("Rotate to point once (3D)"))
            {
                RotateToPointOnce3D();
            }
            if (GUILayout.Button("Rotate to point once (2D)"))
            {
                RotateToPointOnce2D();
            }
        }

        public void OnSceneGUI()
        {
            var tar = (target as LookAtPoint);

            EditorGUI.BeginChangeCheck();
            Vector3 pos = Handles.PositionHandle(tar.lookAtPoint, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Move point");
                tar.lookAtPoint = pos;
                tar.Update(); //Используем Update, описанный в классе tar
            }
        }

        private void RotateToPointOnce3D()
        {
            var tar = (target as LookAtPoint);
            tar.transform.root.LookAt(lookAtPoint.vector3Value);
            Debug.Log($"{tar.transform.root.name} has been rotated to {lookAtPoint.vector3Value}");
        }        
        private void RotateToPointOnce2D()
        {
            Debug.Log("<color=red>RotateToPointOnce2D is Not implemented yet</color>");
        }
    }
}
#endif