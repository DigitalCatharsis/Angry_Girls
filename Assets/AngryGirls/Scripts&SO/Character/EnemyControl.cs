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
            subComponentMediator = GetComponentInChildren<SubComponentMediator>();

            subComponentMediator.OnAwake();
        }
        private void OnEnable()
        {
            CharacterManager.Instance.enemyCharacters.Add(this.gameObject);

            animator = GetComponent<Animator>();
            subComponentMediator.OnComponentEnable();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isDead)
            {
                return;
            }

            subComponentMediator.CheckForDamage(this, SubcomponentMediator_EventNames.CharacterCollider_Trigger_Enter, other);
        }

        private void Update()
        {
            subComponentMediator.OnUpdate();
        }

        private void OnCollisionStay(Collision collision)
        {
            boxColliderContacts = collision.contacts;
        }
        private void FixedUpdate()
        {
            subComponentMediator.OnFixedUpdate();
        }
        private void LateUpdate()
        {
            subComponentMediator.OnLateUpdate();
        }

        private void Start()
        {
            subComponentMediator.OnStart();
        }
    }
}