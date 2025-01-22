using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Angry_Girls
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private SerializedDictionary<CControl, Slider> _healtBar_Dict;
        [SerializeField] private GameObject gameOverUi;
        public void CreateHealthBar(CControl control)
        {
            var healthBar = Resources.Load("HealthBar") as GameObject;
            var HealthBarGameobject = Instantiate(healthBar);
            control.healthSlider = HealthBarGameobject.GetComponentInChildren<Slider>();
            _healtBar_Dict.Add(control, control.healthSlider);
            InitHealthBar(control);
        }

        public void RemoveHealthBar(CControl control)
        {
            _healtBar_Dict.Remove(control);
        }

        private void InitHealthBar(CControl control)
        {
            control.healthSlider.maxValue = control.currentHealth;
            UpdateHealthBarValueAndVision(control);
        }

        public void UpdateHealthBarValueAndVision(CControl control)
        {
            control.healthSlider.value = control.currentHealth;
            if (control.currentHealth <= 0)
            {
                _healtBar_Dict[control].gameObject.SetActive(false);
            }
        }

        public void ShowGameoverUI( )
        {
            gameOverUi.SetActive(true);
        }

        private void LateUpdate()
        {
            foreach (var elem in _healtBar_Dict)
            {
                var rigid = elem.Key.GetComponent<Rigidbody>();
                //var maxY = elem.Key.boxCollider.bounds.max.y + 0.35f;

                elem.Value.transform.root.position = new Vector3(
                    0,
                    rigid.transform.position.y + 1.2f,
                    rigid.transform.position.z
                    //elem.Key.boxCollider.bounds.center.z
                    ) ;

            }
        }

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