using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Angry_Girls
{
    public class UI_Control : MonoBehaviour
    {
        public void Restart()
        {
            StartCoroutine(KillAllTweensAndRestart());

        }
        public void NextScene()
        {
            throw new System.Exception("Next Scene load is not Implemented");
            //SceneManager.LoadScene("test");
        }

        private IEnumerator KillAllTweensAndRestart()
        {
            var tweens = DOTween.PausedTweens() ?? new List<Tween>();
            tweens.AddRange(DOTween.PlayingTweens());
            for (int i = 0; i < tweens.Count; i++)
                tweens[i]?.Kill();
            yield return new WaitForEndOfFrame();

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}