using UnityEngine;

namespace Angry_Girls
{
    public class CameraManager : Singleton<CameraManager>
    {
        [Header("Setup")]
        [SerializeField] private Vector3 cameraStartPosition = new Vector3(13.25f, 1.44f, -0.00999999f);
        [SerializeField] private float startTrthographicCameraSize = 1.65f;

        // Center camera on character collider center
        public void CenterCameraAgainst(Collider selectedObject)
        {
            var boundsCenter = selectedObject.bounds.center;
            Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, boundsCenter.y, boundsCenter.z);
        }

        public void MoveCameraTo(Vector3 placeToMove, float time)
        {
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, placeToMove, time);
        }

        public void ReturnCameraToStartPosition(float speed)
        {
            var lerper = gameObject.AddComponent<MyExtentions>();
            lerper.Lerp_Position(Camera.main.gameObject, startPosition: Camera.main.transform.position, endPosition: cameraStartPosition, speed);
            lerper.Lerp_OrthographicCamera_Size(Camera.main, startValue: Camera.main.orthographicSize, startTrthographicCameraSize, speed);
        }
    }
}