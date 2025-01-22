using UnityEngine;

namespace Angry_Girls
{
    public class CameraSizer : MonoBehaviour
    {
        private const float _targetSizeX = 1920.0f;
        private const float _targetSizeY = 1080.0f;
        private const float _halfSize = 540.0f; // 1/2 of height in pixels

        private void Awake()
        {
            CameraResize();
        }

        private void CameraResize()
        {
            var screenRatio = (float)Screen.width / (float)Screen.height;
            var targetRatio = _targetSizeX / _targetSizeY;

            if (screenRatio < targetRatio)
            {
                Resize();
            }
            else
            {
                {
                    var sizeDiffirence = targetRatio / screenRatio;
                    Resize(sizeDiffirence);
                }
            }
        }

        private void Resize(float sizeDifference = 1.0f)
        {
            Camera.main.orthographicSize = _targetSizeY / _halfSize * sizeDifference;
        }
    }
}