using UnityEngine;

namespace Angry_Girls
{
    public class EnemyControl : CControl
    {

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
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isDead)
            {
                return;
            }

            subComponentProcessor.damageProcessor.CheckForDamage(other);
        }

        private void Update()
        {
            subComponentProcessor.OnUpdate();
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