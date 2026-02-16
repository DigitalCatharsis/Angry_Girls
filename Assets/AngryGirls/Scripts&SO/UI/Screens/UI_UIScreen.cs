using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Base class for all UI screens.
    /// </summary>
    public abstract class UI_UIScreen : MonoBehaviour
    {
        /// <summary>
        /// Show the screen.
        /// </summary>
        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Hide the screen.
        /// </summary>
        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Initialize the screen.
        /// </summary>
        public virtual void Initialize() { }
    }
}