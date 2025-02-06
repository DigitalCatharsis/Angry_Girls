using System.Collections;
using System.Drawing;
using UnityEngine;

namespace Angry_Girls
{
    public class CameraManager : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private readonly Vector3 _cameraStartPosition = new Vector3(13.25f, 1.44f, -0.00999999f);
        [SerializeField] private const float startOrthographicCameraSize = 3f;
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
            allowCameraFollow = true;
        }
        public void StopCameraFollowForRigidBody()
        {
            _characterToFollow = null;
            allowCameraFollow = false;
        }

        // Center camera on character collider center
        private void CenterCameraAgainst(Rigidbody selectedObjectRigidbody)
        {
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

        public void ShakeCamera(float shakeDuration = 0.3f, float shakeMagnitude = 0.05f)
        {
            var originalPosition = Camera.main.transform.localPosition;
            StartCoroutine(ShakeCoroutine(shakeDuration, shakeMagnitude, originalPosition));
        }

        private IEnumerator ShakeCoroutine(float shakeDuration, float shakeMagnitude, Vector3 originalPosition)
        {
            allowCameraFollow = false;
            float elapsed = 0.0f;
            var savedY = Camera.main.transform.localPosition.y;

            while (elapsed < shakeDuration)
            {
                // Генерируем случайное смещение
                float z = Random.Range(-1f, 1f) * shakeMagnitude;
                float y = Random.Range(-1f, 1f) * shakeMagnitude;

                // Применяем смещение к позиции камеры
                Camera.main.transform.localPosition = originalPosition + new Vector3(0, y, z);

                // Увеличиваем время
                elapsed += Time.deltaTime;

                // Ждем следующего кадра
                yield return null;
            }
            Camera.main.transform.localPosition = new Vector3(Camera.main.transform.localPosition.x, savedY, Camera.main.transform.localPosition.z);
            //yield return new WaitForSeconds(0.5f);
            // Возвращаем камеру в исходную позицию
            //Camera.main.transform.localPosition = originalPosition;
            allowCameraFollow = true;
        }


    }
}