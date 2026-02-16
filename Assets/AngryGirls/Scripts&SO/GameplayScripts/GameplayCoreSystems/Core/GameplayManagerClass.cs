using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Base class for all gameplay systems requiring initialization
    /// </summary>
    public abstract class GameplayManagerClass : MonoBehaviour
    {
        public abstract void Initialize();
        protected bool isInitialized;
    }
}