using UnityEngine;

namespace Angry_Girls
{
    public class LookAtPoint : MonoBehaviour
    {
        public GameObject objectToLook;
        public Vector3 lookAtPoint;
        public bool rotateToPoint;
        public Color traceColor;
        public float traceExistDuration = 0.1f;

        public void Update()
        {
            if (traceColor == null) 
            {
                traceColor = Color.magenta;
                traceColor.a = 1;
            }

            if (objectToLook != null)
            {
                lookAtPoint = objectToLook.transform.position;
            }

            if (rotateToPoint == true)
            {
                transform.LookAt(lookAtPoint);
            }
            Debug.DrawLine(transform.position, lookAtPoint, traceColor, traceExistDuration);
        }
    }
}