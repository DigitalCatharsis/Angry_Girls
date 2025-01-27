using UnityEngine;

namespace Angry_Girls
{
    public class HowToPlayUI : MonoBehaviour
    {
        public void GoToNextSlide(GameObject nextSlide)
        {
            nextSlide.SetActive(true);
        }
    }

}
