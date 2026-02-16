using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Calculates and visualizes distance between two objects (Editor only)
    /// </summary>
    [ExecuteInEditMode]
    public class DistanceBetweenTwoObjects : MonoBehaviour
    {
        [Header("Setup")]
        [Space(2)]
        public GameObject farObject;

        [Header("Values")]
        [ShowOnly] public float distanceBetweenObjects;
        [ShowOnly] public float SqrMagnitude;
        [ShowOnly] public Vector3 vectorToObject;

        [Space(10)]
        [ShowOnly] public Vector3 target_WorldPosition;
        [Space(1)]
        [ShowOnly] public Vector3 farObject_WorldPosition;

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
    }
}