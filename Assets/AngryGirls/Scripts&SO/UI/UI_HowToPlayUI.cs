using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Manages "How to Play" tutorial slides
    /// </summary>
    public class UI_HowToPlayUI : MonoBehaviour
    {
        /// <summary>
        /// Activates the next tutorial slide
        /// </summary>
        /// <param name="nextSlide">Next slide GameObject to show</param>
        public void GoToNextSlide(GameObject nextSlide)
        {
            nextSlide.SetActive(true);
        }
    }
}