using UnityEngine;
using UnityEngine.SceneManagement;

namespace Angry_Girls
{

    public class MenuButtonController : MonoBehaviour
    {

        public void Set_SelectButton_bool(GameObject sender)
        {
            var animator = sender.GetComponentInParent<Animator>();
            animator.SetBool("selected", true);
            Debug.Log("Set_SelectButton_bool()" + animator.GetBool("selected"));
        }

        public void ExecuteNewGame(GameObject sender)
        {
            var animator = sender.GetComponentInParent<Animator>();
            animator.SetBool("selected", false);
            animator.SetBool("pressed", true);
            Debug.Log("ExecuteNewGame()");
            Debug.Log("selected() " + animator.GetBool("selected"));
            Debug.Log("pressed() " + animator.GetBool("pressed"));
            SceneManager.LoadScene(1);
        }
        public void Set_isDisselected_bool(GameObject sender)
        {
            var animator = sender.GetComponentInParent<Animator>();
            animator.SetBool("selected", false);
            Debug.Log("Set_isDisselected_bool()" + animator.GetBool("selected"));
        }
    }
}