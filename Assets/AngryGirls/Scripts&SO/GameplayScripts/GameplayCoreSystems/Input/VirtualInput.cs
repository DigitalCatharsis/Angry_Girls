using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Abstract base class for input systems
    /// </summary>
    public abstract class VirtualInput
    {
        public abstract bool IsPressed { get; }
        public abstract bool IsHeld { get; }
        public abstract bool IsReleased { get; }
        public abstract Vector3 Position { get; }
    }
}