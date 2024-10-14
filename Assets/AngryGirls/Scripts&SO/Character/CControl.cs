using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Angry_Girls
{
    public enum PlayerOrAi
    {
        Player,
        Ai,
    }

    [Serializable]
    public class CControl : PoolObject
    {
        [Space(15)]
        [Header("Debug")]
        [Header("State Data")]
        public CurrentStateData currentStateData = new();
        [SerializeField] private CharacterType _characterType;

        [Header("Health")]
        public float currentHealth;
        public Slider healthSlider;

        public bool isLanding = false;
        public bool isGrounded = false;
        public bool isAttacking = false;
        public bool isDead = false;
        public bool unitGotHit = false;
        public bool hasUsedAbility = false;
        public bool hasBeenLaunched = false;
        public bool airToGroundUnit_FinishedAbility = false;
        public bool hasFinishedLaunchingTurn = false;
        public bool hasFinishedAlternateAttackTurn = true;

        [Space(5)]
        public bool unitBehaviorIsAlternate = true;
        public bool checkGlobalBehavior = false;

        public Rigidbody rigidBody;
        public BoxCollider boxCollider;
        public Animator animator;
        public SubComponentMediator subComponentMediator;
        public AttackSystem_Data attackSystem_Data;
        public List<ContactPoint> boxColliderContacts = new();
        [Space(10)]
        public CollisionSpheresData collisionSpheresData;

        [Header("Setup")]
        public PlayerOrAi playerOrAi;
        [Space(5)]
        public CharacterSettings characterSettings;
        [Space(5)]
        [Header("VFX")]
        public Transform projectileSpawnTransform;
        public Color VFX_Color;

        [Header("Weapon")]
        [SerializeReference]
        public Transform weaponHolder;

        public void FinishTurn()
        {
            isAttacking = false;
            isLanding = false;
            hasFinishedAlternateAttackTurn = true;
            airToGroundUnit_FinishedAbility = true;
            hasFinishedLaunchingTurn = true;
        }

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            boxCollider = gameObject.GetComponent<BoxCollider>();
            subComponentMediator = GetComponentInChildren<SubComponentMediator>();

            subComponentMediator.OnAwake();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision == null)
            {
                return;
            }

            boxColliderContacts.AddRange(collision.contacts);
        }

        private void OnCollisionExit(Collision collision)
        {
            foreach (var contact in collision.contacts)
            {
                boxColliderContacts.Remove(contact);
            }
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

            //TEST WEAPON
            //TODO: implement properly
            if (weaponHolder != null)
            {
                var weaponPrefab = Resources.Load("DefinetlyNotAWeapon") as GameObject;
                var weapon = Instantiate(weaponPrefab, weaponHolder.transform.position, Quaternion.identity);

                weapon.transform.parent = weaponHolder;
                weapon.transform.localScale = weaponHolder.transform.localScale;
                weapon.transform.position = weaponHolder.transform.position;
                weapon.transform.rotation = weaponHolder.transform.rotation;
            }
        }

        private void OnEnable()
        {
            if (this is CharacterControl)
            {
                GameLoader.Instance.characterManager.playableCharacters.Add(this.gameObject);
            }
            else if (this is EnemyControl)
            {
                GameLoader.Instance.characterManager.enemyCharacters.Add(this.gameObject);
            }

            subComponentMediator.OnComponentEnable();

            GameLoader.Instance.attackLogicContainer.SetCharacterAttackLogic(this);
            GameLoader.Instance.UIManager.CreateHealthBar(this);
        }


        //ObjectPooling
        protected override void Dispose(bool disposing)
        {
            GameLoader.Instance.UIManager.RemoveHealthBar(this);
            base.Dispose(disposing);
        }

        protected override void ReturnToPool()
        {
            if (!GameLoader.Instance.poolManager.characterPoolDictionary[_characterType].Contains(this))
            {
                GameLoader.Instance.poolManager.AddObject(_characterType, GameLoader.Instance.poolManager.characterPoolDictionary, this);
            }
        }
    }
}