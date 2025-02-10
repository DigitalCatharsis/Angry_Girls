using UnityEngine;

namespace Angry_Girls
{
    public class InputManager: MonoBehaviour
    {
        private VirtualInput _currentInput;

        public InputManager()
        {
            // На мобильных устройствах используем сенсорный ввод
            if (Application.isMobilePlatform)
            {
                _currentInput = new TouchInput();
            }
            else
            {
                _currentInput = new MouseInput();
            }
        }

        public bool IsPressed => _currentInput.IsPressed;
        public bool IsHeld => _currentInput.IsHeld;
        public bool IsReleased => _currentInput.IsReleased;
        public Vector3 Position => _currentInput.Position;

        public float GetZoomDelta()
        {
            if (Application.isMobilePlatform)
            {
                if (Input.touchCount == 2)
                {
                    Touch touch0 = Input.GetTouch(0);
                    Touch touch1 = Input.GetTouch(1);

                    Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
                    Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

                    float prevDistance = (touch0PrevPos - touch1PrevPos).magnitude;
                    float currentDistance = (touch0.position - touch1.position).magnitude;

                    return (currentDistance - prevDistance) * 0.01f;
                }
            }
            else
            {
                return Input.GetAxis("Mouse ScrollWheel");
            }
            return 0;
        }

        public bool IsDragging()
        {
            if (Application.isMobilePlatform)
            {
                return Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved;
            }
            else
            {
                return Input.GetMouseButton(0);
            }
        }

        public Vector2 GetDragDelta()
        {
            if (Application.isMobilePlatform)
            {
                if (Input.touchCount == 1)
                {
                    Touch touch = Input.GetTouch(0);
                    return touch.deltaPosition;
                }
            }
            else
            {
                return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            }
            return Vector2.zero;
        }
    }
}