using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
namespace Angry_Girls
{
    [CustomEditor(typeof(LookAtPoint))]
    [CanEditMultipleObjects]
    public class LookAtPointEditor : Editor
    {
        SerializedProperty lookAtPoint;
        SerializedProperty objectToLook;
        SerializedProperty rotateToPoint;
        SerializedProperty traceColor;
        SerializedProperty traceExistDuration;

        void OnEnable()
        {
            lookAtPoint = serializedObject.FindProperty("lookAtPoint");
            objectToLook = serializedObject.FindProperty("objectToLook");
            rotateToPoint = serializedObject.FindProperty("rotateToPoint");
            traceColor = serializedObject.FindProperty("traceColor");
            traceExistDuration = serializedObject.FindProperty("traceExistDuration");
        }

        public override void OnInspectorGUI()
        {
            var tar = (target as LookAtPoint);
            tar.Update();

            serializedObject.Update();

            EditorGUILayout.PropertyField(lookAtPoint);
            EditorGUILayout.PropertyField(objectToLook);
            EditorGUILayout.PropertyField(rotateToPoint);
            EditorGUILayout.PropertyField(traceColor);
            EditorGUILayout.PropertyField(traceExistDuration);

            if (lookAtPoint.vector3Value.y > (target as LookAtPoint).transform.position.y)
            {
                EditorGUILayout.LabelField("(Selected object is below lookAtPoint)");
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
            EditorGUILayout.LabelField("This is working even if script is disabled!");
            serializedObject.ApplyModifiedProperties();

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
                tar.Update();
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