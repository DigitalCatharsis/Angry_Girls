using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    public class EnemyControl : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private float health = 100f;
        [Header("Debug")]
        [SerializeField] private bool isDead = false;
        private Animator enemyAnimator;

        [SerializeField] private SerializedDictionary<HitReaction_States, int> _hitReaction_Dictionary;
        [SerializeField] private SerializedDictionary<Death_States, int> _death_States_Dictionary;

        private void OnEnable()
        {
            enemyAnimator = GetComponent<Animator>();

            _hitReaction_Dictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<HitReaction_States>(this.gameObject);
            _death_States_Dictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<Death_States>(this.gameObject);
        }

        private void ChangeAnimationState(int newStateHash, int layer = 0, float transitionDuration = 1f)
        {
            enemyAnimator.Play(newStateHash, layer, transitionDuration);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isDead)
            {
                return;
            }

            if (other.gameObject.GetComponent<CharacterControl>() != null)
            {
                var character = other.gameObject.GetComponent<CharacterControl>();
                if (character.isAttacking)
                {
                    ApplyDamage(character);

                    if (health <= 0)
                    {
                        isDead = true;
                        ChangeAnimationState(_death_States_Dictionary[Death_States.A_Sweep_Fall], -1, 1f);

                        return;
                    }

                    ChangeAnimationState(_hitReaction_Dictionary[HitReaction_States.A_HitReaction], 0,1f);
                }
            }
        }

        private void ApplyDamage(CharacterControl control)
        {
            health -= control.characterSettings.attackDamage;
        }

        private void Update()
        {
            if (isDead)
            {
                Debug.Log("Dead");
            }
        }
    }
}