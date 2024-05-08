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

        public void ChangeAnimationState_CrossFadeInFixedTime(int newStateHash, float transitionDuration, int layer = 0, float normalizedTimeOffset = 0.0f, float normalizedTransitionTime = 0.0f)
        {
            if (enemyAnimator.GetCurrentAnimatorClipInfo(0).GetHashCode() == newStateHash)
            {
                return;
            }

            enemyAnimator.CrossFadeInFixedTime(newStateHash, transitionDuration, layer, normalizedTimeOffset, normalizedTransitionTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isDead)
            {
                return;
            }

            if (other.gameObject.transform.root.gameObject.GetComponent<CharacterControl>() != null)
            {
                var character = other.gameObject.transform.root.GetComponent<CharacterControl>();
                if (character.isAttacking)
                {
                    ApplyDamage(character.characterSettings.attackPrepAbility.attackDamage);

                    if (health <= 0)
                    {
                        isDead = true;
                        ChangeAnimationState_CrossFadeInFixedTime(_death_States_Dictionary[Death_States.A_Sweep_Fall], 0.05f);

                        return;
                    }

                    ChangeAnimationState_CrossFadeInFixedTime(_hitReaction_Dictionary[HitReaction_States.A_HitReaction], 0.05f);
                }
            }
            if (other.gameObject.transform.root.GetComponent<VFX>() != null)
            {
                var vfx = other.gameObject.transform.root.GetComponent<VFX>();

                ApplyDamage(vfx.projectileDamage);

                if (health <= 0)
                {
                    isDead = true;
                    ChangeAnimationState_CrossFadeInFixedTime(_death_States_Dictionary[Death_States.A_Sweep_Fall], 0.05f);

                    return;
                }

                ChangeAnimationState_CrossFadeInFixedTime(_hitReaction_Dictionary[HitReaction_States.A_HitReaction], 0.05f);
            }
        }

        private void ApplyDamage(float damage)
        {
            health -= damage;
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