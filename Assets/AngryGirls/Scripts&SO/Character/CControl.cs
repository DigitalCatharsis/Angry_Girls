using System.Collections;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    public enum PlayerOrAi
    {
        Character,
        Bot,
    }

    public class CControl : PoolObject
    {
        public CharacterHealth Health { get; private set; }
        public CharacterMovement CharacterMovement { get; private set; }
        public Ragdoll Ragdoll { get; private set; }

        [Header("Health")]
        public Slider healthSlider;

        private const float _finishTurnTimerValue = 3f;

        public bool isLanding = false;
        public bool isGrounded = false;
        public bool isAttacking = false;
        public bool isDead = false;
        public bool unitGotHit = false;
        public bool hasUsedAbility = false;
        public bool canUseAbility = false;
        public bool hasBeenLaunched = false;
        public bool hasFinishedLaunchingTurn = false;
        public bool hasFinishedAlternateAttackTurn = true;

        [Space(5)]
        public bool unitBehaviorIsAlternate = true;
        public bool checkGlobalBehavior = false;

        public SubComponentMediator subComponentMediator;
        private SubComponentsController subComponentsController;

        public BoxCollider boxCollider;
        public Animator animator;
        public AttackSystem_Data attackSystem_Data;

        [Header("Setup")]
        public PlayerOrAi playerOrAi;
        [Space(5)]
        public CharacterSettings characterSettings;
        [Space(5)]
        [Header("VFX")]
        public Transform projectileSpawnTransform;
        public Transform wingsTransform;
        public UnityEngine.Color vfxColor;

        [Header("Weapon")]
        public Transform weaponHolder;

        public void SetDeathParams()
        {
            ColorDebugLog.Log($"{this.name} called death", KnownColor.Yellow);
            isDead = true;
            FinishTurn(0);
            CharacterMovement.Rigidbody.constraints = RigidbodyConstraints.None;
            CharacterMovement.Rigidbody.useGravity = true;
            CharacterMovement.Rigidbody.isKinematic = false;
            CharacterMovement.Rigidbody.interpolation = RigidbodyInterpolation.None;
            CharacterMovement.Rigidbody.detectCollisions = false;
            boxCollider.enabled = false;
            

            var animator = wingsTransform.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.enabled = false;
            }

            gameObject.layer = LayerMask.NameToLayer("DeadBody");
        }

        private void OnEnable()
        {
            if (playerOrAi == PlayerOrAi.Character)
            {
                GameLoader.Instance.characterManager.playableCharacters.Add(this);
            }
            else if (playerOrAi == PlayerOrAi.Bot)
            {
                GameLoader.Instance.characterManager.enemyCharacters.Add(this);
            }

            subComponentsController.OnComponentEnable();
            GameLoader.Instance.attackLogicContainer.SetCharacterAttackLogic(this);
            GameLoader.Instance.gameLogic_UIManager.CreateHealthBar(this);
        }

        private void Awake()
        {
            Ragdoll = GetComponent<Ragdoll>();

            Health = GetComponent<CharacterHealth>();
            Health.Initialize();

            CharacterMovement = GetComponent<CharacterMovement>();

            animator = GetComponent<Animator>();
            boxCollider = GetComponent<BoxCollider>();
            subComponentMediator = GetComponentInChildren<SubComponentMediator>();
            subComponentsController = GetComponentInChildren<SubComponentsController>();
            subComponentMediator.OnAwake();
            subComponentsController.OnAwake();

            GameLoader.Instance.interactionManager.Register(gameObject, new InteractionConfig
            {
                type = InteractionMemberType.Character,
                ownerGO = gameObject
            });
        }

        public AttackAbilityLogic Get_AttackFinish_AttackAbilityLogic()
        {
            if (GameLoader.Instance.turnManager.CurrentPhase == CurrentPhase.LaunchingPhase)
            {
                return attackSystem_Data.launch_AttackFinishLogic;
            }
            else
            {
                return attackSystem_Data.alternate_AttackFinishLogic;
            }
        }

        public AttackAbilityData Get_AttackAbility()
        {
            if (GameLoader.Instance.turnManager.CurrentPhase == CurrentPhase.LaunchingPhase)
            {
                return characterSettings.AttackAbility_Launch;
            }
            else
            {
                return characterSettings.AttackAbility_Alternate;
            }
        }

        public bool CheckAttackFinishCondition()
        {
            if (isDead)
            {
                return false;
            }

            if (unitGotHit)
            {
                Debug.Log($"Returning True in CheckAttackFinishCondition cause of unitGotHit for {this.name} in animation {GetCurrentAnimationName()}");
                return true;
            }

            if (characterSettings.unitType == UnitType.Air && hasUsedAbility == true)
            {
                return true;
            }

            if (GameLoader.Instance.turnManager.CurrentPhase == CurrentPhase.AlternatePhase)
            {
                if (hasFinishedAlternateAttackTurn)
                {
                    return false;
                }

                if (canUseAbility)
                {
                    return false;
                }
            }

            if (isGrounded)
            {
                return true;
            }

            return false;
        }

        public void FinishTurn(float finishAttackTimer = _finishTurnTimerValue)
        {
            Debug.Log(this.name + " starting finishing turn");
            canUseAbility = false;
            StopCoroutine(ExecuteFinishTurnTimer(finishAttackTimer));
            isAttacking = false;
            StartCoroutine(ExecuteFinishTurnTimer(finishAttackTimer));
        }

        private IEnumerator ExecuteFinishTurnTimer(float timeToCheck)
        {
            Debug.Log(this.name + " starting finishing corutine");
            var time = Time.deltaTime;
            while (time >= timeToCheck)
            {
                time += Time.deltaTime;
                yield return null;
            }

            hasFinishedLaunchingTurn = true;
            hasFinishedAlternateAttackTurn = true;
            Debug.Log(this.name + " has fininshed AttackFinishing corutine");
            yield break;
        }

        private void FixedUpdate()
        {
            subComponentsController.OnFixedUpdate();
        }

        public int GetVfxLayermask()
        {
            if (playerOrAi == PlayerOrAi.Character)
            {
                return LayerMask.NameToLayer("Projectile_Character");
            }
            else
            {
                return LayerMask.NameToLayer("Projectile_Bot");
            }
        }

        //for debug 
        public string GetCurrentAnimationName()
        {
            var currentHash = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            return GameLoader.Instance.statesContainer.GetStateNameByHash(currentHash);
        }

        private void Update()
        {
            subComponentsController.OnUpdate();
        }

        private void LateUpdate()
        {
            subComponentsController.OnLateUpdate();
        }

        private void Start()
        {
            subComponentsController.OnStart();

            if (weaponHolder != null)
            {
                var weaponPrefab = Resources.Load("DefinetlyNotAWeapon") as GameObject;
                var weapon = Instantiate(weaponPrefab, weaponHolder.transform.position, Quaternion.identity);

                weapon.gameObject.layer = this.gameObject.layer;
                foreach (Transform child in weapon.transform)
                {
                    child.gameObject.layer = this.gameObject.layer;
                }

                weapon.transform.parent = weaponHolder;
                weapon.transform.localScale = weaponHolder.transform.localScale;
                weapon.transform.position = weaponHolder.transform.position;
                weapon.transform.rotation = weaponHolder.transform.rotation;
            }
        }

        protected override void OnDispose()
        {
            GameLoader.Instance.interactionManager.CleanUpForOwner(gameObject);
            GameLoader.Instance.gameLogic_UIManager.RemoveHealthBar(this);
        }

        protected override void ReturnToPool()
        {
            if (!GameLoader.Instance.poolManager.characterPoolDictionary[characterSettings.characterType].Contains(this))
            {
                GameLoader.Instance.poolManager.AddObject(characterSettings.characterType, GameLoader.Instance.poolManager.characterPoolDictionary, this);
            }
        }

        //TODO: implement ragdoll
        public void TempOnDeathZone()
        {
            CharacterMovement.Rigidbody.isKinematic = true;
            CharacterMovement.Rigidbody.velocity = Vector3.zero;
        }

        public bool IsAlly(CControl anotherControl)
        {
            return playerOrAi == anotherControl.playerOrAi;
        }
    }
}