using AYellowpaper.SerializedCollections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    public class GameLogic_UIManager : MonoBehaviour
    {
        [Header("Setup)")]
        [SerializeField] private GameObject gameOverUi;
        [SerializeField] private GameObject scoreUi;

        [Header("Deubg")]
        [SerializeField] private SerializedDictionary<CControl, Slider> _healtBar_Dict;
        [SerializeField] private int _score = 0;
        [SerializeField] private GameObject _healthBarsContainer;


        public void CreateHealthBar(CControl control)
        {
            if (_healthBarsContainer == null)
            {
                _healthBarsContainer = new GameObject("HealthBarContainer");
            }

            var healthBar = Resources.Load("HealthBar") as GameObject;
            var healthBarGameobject = Instantiate(healthBar);
            healthBarGameobject.name = "HealthBar_" + control.name;
            healthBarGameobject.transform.SetParent(_healthBarsContainer.transform, true);
            control.healthSlider = healthBarGameobject.GetComponentInChildren<Slider>();
            _healtBar_Dict.Add(control, control.healthSlider);

            InitHealthBar(control);
        }

        public void RemoveHealthBar(CControl control)
        {
            _healtBar_Dict.Remove(control);
        }       

        public void DisableHealthBar(CControl control)
        {
            _healtBar_Dict[control].gameObject.SetActive(false);
        }

        private void InitHealthBar(CControl control)
        {
            control.healthSlider.maxValue = control.Health.CurrentHealth;
            UpdateHealthBarValueAndVision(control);
        }

        public void UpdateHealthBarValueAndVision(CControl control)
        {
            control.healthSlider.value = control.Health.CurrentHealth;
        }

        public void UpdateScore(int value)
        {
            _score += value;
            scoreUi.GetComponentInChildren<TextMeshProUGUI>().text = $"Score: {_score}";
        }

        private void Start()
        {
            UpdateScore(0);
        }

        private void LateUpdate()
        {
            UpdateHealthBarsPositions();
        }

        private void UpdateHealthBarsPositions()
        {
            foreach (var elem in _healtBar_Dict)
            {
                var rigid = elem.Key.GetComponent<Rigidbody>();

                elem.Value.transform.parent.position = new Vector3(
                    0,
                    rigid.transform.position.y + 1.2f,
                    rigid.transform.position.z
                    );
            }
        }

        public void ShowGameoverUI()
        {
            gameOverUi.SetActive(true);
        }
    }
}