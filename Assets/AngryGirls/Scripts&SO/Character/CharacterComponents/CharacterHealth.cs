using System;
using UnityEngine;

namespace Angry_Girls
{
    public class CharacterHealth : MonoBehaviour
    {
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _currentHealth;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth; 

        public event Action<float> OnHealthChanged;
        public event Action OnDeath;

        public void Initialize(float health)
        {
            _maxHealth = health;
            _currentHealth = _maxHealth;
        }

        public void ApplyDamage(float damage)
        {
            _currentHealth = Mathf.Max(0, _currentHealth - damage);
            OnHealthChanged?.Invoke(_currentHealth);

            if (_currentHealth <= 0)
            {
                OnDeath?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            OnHealthChanged?.Invoke(_currentHealth);
        }
    }
}