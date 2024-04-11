using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Angry_Girls
{
    [ExecuteInEditMode]
    public class DistanceBetweenTwoObjects : MonoBehaviour
    {
        [Header("Setup")]
        [Space(2)]
        public GameObject farObject;

        [Header("Values")]
        [ShowOnlyAttribute] public float distanceBetweenObjects;
        [ShowOnlyAttribute] public float SqrMagnitude;
        [ShowOnlyAttribute] public Vector3 vectorToObject;

        [Space(10)]
        [ShowOnlyAttribute] public Vector3 target_WorldPosition;
        [Space(1)]
        [ShowOnlyAttribute] public Vector3 farObject_WorldPosition;

        private void Update()
        {
            target_WorldPosition = transform.position;

            if (farObject != null)
            {
                distanceBetweenObjects = Vector3.Distance(transform.position, farObject.transform.position);
                Debug.DrawLine(transform.position, farObject.transform.position, Color.green);

                SqrMagnitude = Vector3.SqrMagnitude(farObject.transform.position - transform.position);
                vectorToObject = farObject.transform.position - transform.position;

                farObject_WorldPosition = transform.position;
            }
        }

        //private void OnDrawGizmos()
        //{
        //    if (farObject != null)
        //    {
        //        GUI.color = Color.black;
        //        Handles.Label(transform.position - (transform.position -
        //        farObject.transform.position) / 2, distanceBetweenObjects.ToString());
        //    }
        //}
    }
}