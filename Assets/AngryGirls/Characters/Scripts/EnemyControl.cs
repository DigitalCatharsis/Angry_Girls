using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Angry_Girls
{
    public class EnemyControl : CControl
    {
        [Header("Setup")]
        [SerializeField] private float health = 100f;
        [Header("Debug")]

        [SerializeField] private SerializedDictionary<HitReaction_States, int> _hitReaction_Dictionary;
        [SerializeField] private SerializedDictionary<Death_States, int> _death_States_Dictionary;

        private void Awake()
        {
            subComponentProcessor = GetComponentInChildren<SubComponentProcessor>();
            subComponentProcessor.OnAwake();
        }

        private void OnEnable()
        {
            Singleton.Instance.characterManager.playableCharacters.Add(this.gameObject);
            Singleton.Instance.characterManager.enemyCharacters.Add(this.gameObject);

            animator = GetComponent<Animator>();
            subComponentProcessor.OnComponentEnable();

            _hitReaction_Dictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<HitReaction_States>(this.gameObject);
            _death_States_Dictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<Death_States>(this.gameObject);

            currentHealth = health;
        }

        //private void ChangeAnimationState(int newStateHash, int layer = 0, float transitionDuration = 1f)
        //{
        //    animator.Play(newStateHash, layer, transitionDuration);
        //}

        public void ChangeAnimationState_CrossFadeInFixedTime(int newStateHash, float transitionDuration, int layer = 0, float normalizedTimeOffset = 0.0f, float normalizedTransitionTime = 0.0f)
        {
            if (animator.GetCurrentAnimatorClipInfo(0).GetHashCode() == newStateHash)
            {
                return;
            }

            animator.CrossFadeInFixedTime(newStateHash, transitionDuration, layer, normalizedTimeOffset, normalizedTransitionTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isDead)
            {
                return;
            }

            if (other.gameObject.transform.GetComponent<VFX>() != null)
            {
                var vfx = other.gameObject.transform.GetComponent<VFX>();

                ApplyDamage(vfx.projectileDamage);

                if (currentHealth <= 0)
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
            currentHealth -= damage;
        }

        private void Update()
        {
            subComponentProcessor.OnUpdate();

            if (isDead)
            {
                Debug.Log("Dead");
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            boxColliderContacts = collision.contacts;
        }
        private void FixedUpdate()
        {
            subComponentProcessor.OnFixedUpdate();
        }
        private void LateUpdate()
        {
            subComponentProcessor.OnLateUpdate();
        }

        private void Start()
        {
            subComponentProcessor.OnStart();
        }
    }
}