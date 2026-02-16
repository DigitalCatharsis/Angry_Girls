using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Base class for all gameplay UI components with lifecycle management
    /// </summary>
    public abstract class UI_GameplayManagersComponent : MonoBehaviour
    {
        protected bool isInitialized;

        public virtual void Initialize() => isInitialized = true;
        public virtual void Show() => gameObject.SetActive(true);
        public virtual void Hide() => gameObject.SetActive(false);

        protected virtual void LateUpdate() { }
    }
}