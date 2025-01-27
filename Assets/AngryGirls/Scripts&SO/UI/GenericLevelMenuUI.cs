using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Angry_Girls
{
    public class GenericLevelMenuUI : MonoBehaviour
    {
        public void RestartCurrentLevel()
        {
            StartCoroutine(KillAllTweensAndRestart());

        }
        public void BackToMainMenu()
        {
            StartCoroutine(KillAllTweensAnLoadLevel(0));
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
        private IEnumerator KillAllTweensAnLoadLevel(int levelIndex)
        {
            var tweens = DOTween.PausedTweens() ?? new List<Tween>();
            tweens.AddRange(DOTween.PlayingTweens());
            for (int i = 0; i < tweens.Count; i++)
                tweens[i]?.Kill();
            yield return new WaitForEndOfFrame();

            SceneManager.LoadScene(levelIndex);
        }
    }
}