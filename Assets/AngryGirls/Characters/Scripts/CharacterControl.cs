using UnityEngine;

namespace Angry_Girls
{
    public class CharacterControl : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private bool _isPlayer = true;
        public CharacterSettings characterSettings;

        [Space(15)]
        [Header("Debug")]
        public Animator animator;
        public Rigidbody rigidBody;
        public BoxCollider boxCollider;
        public SubComponentProcessor subComponentProcessor;
        public ContactPoint[] boxColliderContacts;

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            boxCollider = gameObject.GetComponent<BoxCollider>();
            subComponentProcessor = GetComponentInChildren<SubComponentProcessor>();

            subComponentProcessor.OnAwake();

        }

        private void OnCollisionStay(Collision collision)
        {
            boxColliderContacts = collision.contacts;
        }

        private void Update()
        {
            subComponentProcessor.OnUpdate();
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
        private void OnEnable()
        {
            if (_isPlayer == true)
            {
                Singleton.Instance.characterManager.playableCharacters.Add(this.gameObject);
            }
            else
            {
                Singleton.Instance.characterManager.enemyCharacters.Add(this.gameObject);
            }

            subComponentProcessor.OnComponentEnable();
        }
    }
}