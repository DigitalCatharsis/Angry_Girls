using UnityEngine;
using UnityEngine.SceneManagement;

namespace Angry_Girls
{
    public class UI_Control : MonoBehaviour
    {
        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}