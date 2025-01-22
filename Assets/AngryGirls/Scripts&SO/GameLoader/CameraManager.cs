using System.Drawing;
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

        [SerializeField] private Rigidbody _characterToFollow;
        [SerializeField] private bool allowCameraFollow = false;

        private void LateUpdate()
        {
            if (_characterToFollow != null && allowCameraFollow)
            {
                CenterCameraAgainst(_characterToFollow);
            }
        }

        public void CameraFollowForRigidBody(Rigidbody characterToFollow)
        {
            ColorDebugLog.Log("Called for FollowCamera", KnownColor.Orange);
            _characterToFollow = characterToFollow.GetComponent<Rigidbody>();
            allowCameraFollow  = true;
        }
        public void StopCameraFollowForRigidBody()
        {
            _characterToFollow = null;
            allowCameraFollow = false;
        }

        // Center camera on character collider center
        private void CenterCameraAgainst(Rigidbody selectedObjectRigidbody)
        {
            //ColorDebugLog.Log("Called CenterCamera", KnownColor.Orange);
            //if (selectedObject.GetComponent<Collider>() == null)
            //{
            //    Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, selectedObject.transform.position.y, selectedObject.transform.position.z);
            //    return;
            //}
            //var boundsCenter = selectedObject.GetComponent<Collider>().bounds.center;
            Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, selectedObjectRigidbody.transform.position.y, selectedObjectRigidbody.transform.position.z);
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
            StopCameraFollowForRigidBody();
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