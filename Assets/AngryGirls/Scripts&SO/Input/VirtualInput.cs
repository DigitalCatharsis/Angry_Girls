using UnityEngine;

namespace Angry_Girls
{
    public abstract class VirtualInput
    {
        public abstract bool IsPressed { get; }
        public abstract bool IsHeld { get; }
        public abstract bool IsReleased { get; }
        public abstract Vector3 Position { get; }
    }
}