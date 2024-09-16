using System.Linq;
using UnityEngine;

namespace Angry_Girls
{
    public enum PlayerOrAi
    {
        Player,
        Ai,
    }
    public abstract class CControl : PoolObject
    {
        [Space(15)]
        [Header("Debug")]
        [Header("State Data")]
        public CurrentStateData currentStateData = new();
        [SerializeField] private  CharacterType _characterType;

        [Header("Health")]
        public float currentHealth;

        public bool isLanding = false;
        public bool isGrounded = false;
        public bool isAttacking = false;
        public bool isDead = false;
        public bool unitGotHit = false;
        public bool hasUsedAbility = false;
        public bool hasBeenLaunched = false;
        public bool airToGroundUnit_FinishedAbility = false;
        public bool hasFinishedLaunchingTurn = false; 
        public bool hasFinishedStaticAttackTurn = true;

        [Space(5)]
        public bool unitBehaviorIsStatic = true; 
        public bool checkGlobalBehavior = false;

        public Rigidbody rigidBody;
        public BoxCollider boxCollider;
        public Animator animator;
        public SubComponentMediator subComponentMediator;
        public AttackSystem_Data attackSystem_Data;
        public ContactPoint[] boxColliderContacts;
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

        protected override void Dispose(bool disposing)
        {
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