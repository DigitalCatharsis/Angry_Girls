using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Angry_Girls
{
    public class MenuButtonController : MonoBehaviour
    {
        [SerializeField] private GameObject _mainmenu;
        [SerializeField] private GameObject _options;
        [SerializeField] private GameObject _pauseMenu;
        [SerializeField] private GameObject _gameUi;

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
            if (DOTween.PlayingTweens() != null)
            {
                tweens.AddRange(DOTween.PlayingTweens());
                for (int i = 0; i < tweens.Count; i++)
                    tweens[i]?.Kill();
            }
            yield return new WaitForEndOfFrame();

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private IEnumerator KillAllTweensAnLoadLevel(int levelIndex)
        {
            DOTween.KillAll(); // Полная очистка всех твинов
            //GameLoader.Instance?.Dispose();
            //var tweens = DOTween.PausedTweens() ?? new List<Tween>();

            //if (DOTween.PlayingTweens() != null)
            //{
            //    tweens.AddRange(DOTween.PlayingTweens());
            //    for (int i = 0; i < tweens.Count; i++)
            //        tweens[i]?.Kill();
            //}

            yield return new WaitForEndOfFrame();

            SceneManager.LoadScene(levelIndex, LoadSceneMode.Single);
        }

        public void SelectButton(GameObject sender)
        {
            var animator = sender.GetComponentInParent<Animator>();
            animator.SetBool("selected", true);
        }

        public void DisselectButton(GameObject sender)
        {
            var animator = sender.GetComponentInParent<Animator>();
            animator.SetBool("selected", false);
        }

        public void OpenPauseMenu()
        {
            GameLoader.Instance.pauseControl.PauseGame();
            _pauseMenu.SetActive(true);
            _gameUi.SetActive(false);
        }

        public void OpenOptionMenu()
        {
            _mainmenu.SetActive(false);
            _options.SetActive(true);
        }

        public void ReturnToMainMenu(GameObject self)
        {
            self.SetActive(false);
            _mainmenu.SetActive(true);
        }

        public void ClosePausePanelAndReturnToGame()
        {
            _pauseMenu.SetActive(false);
            _gameUi.SetActive(true);
            GameLoader.Instance.pauseControl.UnpauseGame();
        }


        public void NewGame()
        {
            // Получаем индекс текущей сцены
            //int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            StartCoroutine(KillAllTweensAnLoadLevel(1));
            // Загружаем следующую сцену
            //SceneManager.LoadScene(1);
            //SceneManager.LoadScene(1);
        }

        public void ExitGame()
        {
        #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
        #else
                        Application.Quit();
        #endif
        }
    }
}
