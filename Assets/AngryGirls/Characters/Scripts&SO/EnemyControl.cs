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
            rigidBody = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            boxCollider = gameObject.GetComponent<BoxCollider>();
            subComponentProcessor = GetComponentInChildren<SubComponentProcessor>();

            subComponentProcessor.OnAwake();
        }

        private void OnEnable()
        {
            Singleton.Instance.characterManager.enemyCharacters.Add(this.gameObject);

            animator = GetComponent<Animator>();
            subComponentProcessor.OnComponentEnable();

            _hitReaction_Dictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<HitReaction_States>(this.gameObject);
            _death_States_Dictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<Death_States>(this.gameObject);

            currentHealth = health;
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
                    subComponentProcessor.animationProcessor.ChangeAnimationState_CrossFadeInFixedTime(_death_States_Dictionary[Death_States.A_Sweep_Fall], 0.05f);

                    return;
                }

                subComponentProcessor.animationProcessor.ChangeAnimationState_CrossFadeInFixedTime(_hitReaction_Dictionary[HitReaction_States.A_HitReaction], 0.05f);
            }
        }

        private void ApplyDamage(float damage)
        {
            currentHealth -= damage;
        }

        private void Update()
        {
            subComponentProcessor.OnUpdate();

            //if (isDead)
            //{
            //    Debug.Log("Dead");
            //}
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