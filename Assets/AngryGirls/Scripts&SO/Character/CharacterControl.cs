using UnityEngine;

namespace Angry_Girls
{
    public class CharacterControl : CControl
    {
        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            boxCollider = gameObject.GetComponent<BoxCollider>();
            subComponentMediator = GetComponentInChildren<SubComponentMediator>();

            subComponentMediator.OnAwake();
        }

        private void OnCollisionStay(Collision collision)
        {
            boxColliderContacts = collision.contacts;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isDead)
            {
                return;
            }

            //TODO: ask Pasha
            subComponentMediator.CheckForDamage(this, SubcomponentMediator_EventNames.CharacterCollider_Trigger_Enter, other);
        }

        private void Update()
        {
            subComponentMediator.OnUpdate();
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
        private void OnEnable()
        {
            GameLoader.Instance.characterManager.playableCharacters.Add(this.gameObject);

            subComponentMediator.OnComponentEnable();
        }
        private void OnTriggerExit(Collider other)
        {

        }
    }
}