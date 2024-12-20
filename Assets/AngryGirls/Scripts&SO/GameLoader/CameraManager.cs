using UnityEngine;

namespace Angry_Girls
{
    public class CameraManager : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private readonly Vector3 _cameraStartPosition = new Vector3(13.25f, 1.44f, -0.00999999f);
        [SerializeField] private const float startOrthographicCameraSize = 1.85f;
        [SerializeField] private const float _secondsCameraWaitsAfterAttack = 2f;
        [SerializeField] private const float _zoomeCameraValueAfterLaunch = 5f;

        public float SecondsCameraWaitsAfterAttack => _secondsCameraWaitsAfterAttack;

        // Center camera on character collider center
        public void CenterCameraAgainst(GameObject selectedObject)
        {
            if (selectedObject.GetComponent<Collider>() == null)
            {
                Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, selectedObject.transform.position.y, selectedObject.transform.position.z);
                return;
            }
            var boundsCenter = selectedObject.GetComponent<Collider>().bounds.center;
            Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, boundsCenter.y, boundsCenter.z);
        }

        public void ZoomOutCameraAfterLaunch()
        {
             Camera.main.orthographicSize -= (Camera.main.orthographicSize / _zoomeCameraValueAfterLaunch);  
        }

        public void MoveCameraTo(Vector3 placeToMove, float speed)
        {
            GameLoader.Instance.myExtentions.Lerp_Position(Camera.main.gameObject, startPosition: Camera.main.transform.position, endPosition: placeToMove, speed);
            GameLoader.Instance.myExtentions.Lerp_OrthographicCamera_Size(Camera.main, startValue: Camera.main.orthographicSize, startOrthographicCameraSize, speed);
        }

        public void ReturnCameraToStartPosition(float speed)
        {
            GameLoader.Instance.myExtentions.Lerp_Position(Camera.main.gameObject, startPosition: Camera.main.transform.position, endPosition: _cameraStartPosition, speed);
            GameLoader.Instance.myExtentions.Lerp_OrthographicCamera_Size(Camera.main, startValue: Camera.main.orthographicSize, startOrthographicCameraSize, speed);
        }

        public Vector3 GetPointerWorldPosition(Camera camera)
        {
            Vector3 screenPosition = Input.mousePosition;
            screenPosition.z = camera.nearClipPlane + 1;
            return camera.ScreenToWorldPoint(screenPosition);
        }
    }
}