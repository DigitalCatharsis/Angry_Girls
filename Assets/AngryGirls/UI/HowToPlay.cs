using UnityEngine;

namespace Angry_Girls
{
    public class HowToPlay : MonoBehaviour
    {
        public void GoToNextSlide(GameObject nextSlide)
        {
            nextSlide.SetActive(true);
        }
    }

}
