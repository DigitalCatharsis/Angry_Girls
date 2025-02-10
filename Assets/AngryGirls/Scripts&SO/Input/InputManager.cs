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
    }
}