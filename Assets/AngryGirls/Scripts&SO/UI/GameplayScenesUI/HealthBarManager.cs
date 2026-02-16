using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// Manages health bar creation, positioning and updates for all characters
    /// </summary>
    public class HealthBarManager : UI_GameplayManagersComponent
    {
        [SerializeField] private GameObject _healthBarPrefab;
        [SerializeField] private float _healthbatUpDelta = 1.21f;
        [SerializeField] private float _healthJumping—oefficient = 0.005f; //the less - the better

        private Dictionary<CControl, Slider> _healthBars = new();
        private GameplayCharactersManager _charactersManager;
        

        public override void Initialize()
        {

            _charactersManager = GameplayCoreManager.Instance.GameplayCharactersManager;

            foreach (var character in _charactersManager.AllCharacters)
            {
                RegisterCharacter(character);
            }
            base.Initialize();
            isInitialized = true;
        }

        private void RegisterCharacter(CControl character)
        {
            if (_healthBars.ContainsKey(character)) return;

            var healthBarObj = Instantiate(_healthBarPrefab);
            healthBarObj.GetComponent<Canvas>().worldCamera = Camera.main;
            healthBarObj.name = $"HealthBar_{character.name}";
            var slider = healthBarObj.GetComponentInChildren<Slider>();
            var text = healthBarObj.GetComponentInChildren<TextMeshProUGUI>();

            slider.maxValue = character.Health.MaxHealth;
            slider.value = character.Health.CurrentHealth;
            text.text = $"{(int)character.Health.CurrentHealth}/{(int)character.Health.MaxHealth}";

            character.Health.OnHealthChanged += (newHealth) =>
            {
                slider.value = newHealth;
                text.text = $"{(int)newHealth}/{(int)character.Health.MaxHealth}";
            };

            character.Health.OnDeath += () =>
            {
                slider.gameObject.SetActive(false);
            };

            _healthBars[character] = slider;
        }

        protected override void LateUpdate()
        {
            if (!isInitialized) return;

            foreach (var entry in _healthBars)
            {
                var character = entry.Key;
                var slider = entry.Value;

                if (!character.isDead && slider.gameObject.activeSelf)
                {
                    float speedY = Mathf.Abs(character.CharacterMovement.Rigidbody.velocity.y);

                    // Smooth displacement transition: at speeds below healthJumpingCoefficient, coefficient = 1,
                    // at speeds above healthJumpingCoefficient*2, coefficient = 0
                    float thresholdLow = _healthJumping—oefficient;
                    float thresholdHigh = _healthJumping—oefficient * 2f;
                    float t = Mathf.InverseLerp(thresholdHigh, thresholdLow, speedY);
                    t = Mathf.Clamp01(t);

                    // Base displacement (sinusoidal)
                    float verticalOffset = Mathf.Sin(character.transform.position.z * 2f) * 0.2f;
                    // Apply the coefficient
                    verticalOffset *= t;

                    Vector3 targetPos = character.transform.position + Vector3.up * (_healthbatUpDelta + verticalOffset);
                    slider.transform.position = targetPos;
                }
            }
        }
    }
}