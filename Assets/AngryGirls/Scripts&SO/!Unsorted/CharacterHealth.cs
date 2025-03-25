using System;
using UnityEngine;

namespace Angry_Girls
{
    public class CharacterHealth : MonoBehaviour
    {
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _currentHealth;

        public float CurrentHealth => _currentHealth;
        public float NormalizedHealth => _currentHealth / _maxHealth;

        public void Initialize()
        {
            _currentHealth = _maxHealth;
        }

        public void ApplyDamage(float damage)
        {
            _currentHealth = Mathf.Max(0, _currentHealth - damage);
        }

        public void Heal(float amount)
        {
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
        }
    }
}