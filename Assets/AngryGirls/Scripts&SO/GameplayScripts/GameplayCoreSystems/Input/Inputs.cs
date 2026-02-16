using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Mouse input implementation for desktop
    /// </summary>
    public class MouseInput : VirtualInput
    {
        public override bool IsPressed => Input.GetMouseButtonDown(0);
        public override bool IsHeld => Input.GetMouseButton(0);
        public override bool IsReleased => Input.GetMouseButtonUp(0);
        public override Vector3 Position => Input.mousePosition;
    }

    /// <summary>
    /// Touch input implementation for mobile
    /// </summary>
    public class TouchInput : VirtualInput
    {
        public override bool IsPressed => Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
        public override bool IsHeld => Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary;
        public override bool IsReleased => Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended;
        public override Vector3 Position => Input.GetTouch(0).position;
    }
}