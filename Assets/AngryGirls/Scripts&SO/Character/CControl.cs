using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    public enum PlayerOrAi
    {
        Player,
        Ai,
    }

    public class CControl : PoolObject
    {
        public CharacterHealth Health { get; private set; }
        public CharacterMovement CharacterMovement { get; private set; }

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
        public UnityEngine.Color VFX_Color;

        [Header("Weapon")]
        public Transform weaponHolder;

        private void OnEnable()
        {
            if (playerOrAi == PlayerOrAi.Player)
            {
                GameLoader.Instance.characterManager.playableCharacters.Add(this);
            }
            else if (playerOrAi == PlayerOrAi.Ai)
            {
                GameLoader.Instance.characterManager.enemyCharacters.Add(this);
            }

            subComponentsController.OnComponentEnable();

            GameLoader.Instance.attackLogicContainer.SetCharacterAttackLogic(this);
            GameLoader.Instance.gameLogic_UIManager.CreateHealthBar(this);
        }

        private void Awake()
        {
            Health = GetComponent<CharacterHealth>();
            Health.Initialize();

            CharacterMovement = GetComponent<CharacterMovement>();
            CharacterMovement.Initialize(this);

            animator = GetComponent<Animator>();
            boxCollider = gameObject.GetComponent<BoxCollider>();
            subComponentMediator = GetComponentInChildren<SubComponentMediator>();
            subComponentsController = GetComponentInChildren<SubComponentsController>();
            subComponentMediator.OnAwake();
            subComponentsController.OnAwake();
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
            canUseAbility = false;
            StopCoroutine(ExecuteFinishTurnTimer(finishAttackTimer));
            isAttacking = false;

            //ColorDebugLog.Log("ExecuteFinishTurnTimer" + gameObject.name, KnownColor.Yellow);
            //check for calls. Has been fixed, but to be sure
            StartCoroutine(ExecuteFinishTurnTimer(finishAttackTimer));
        }

        private IEnumerator ExecuteFinishTurnTimer(float timeToCheck)
        {
            var time = Time.deltaTime;
            while (time >= timeToCheck)
            {
                time += Time.deltaTime;
                yield return null;
            }

            hasFinishedLaunchingTurn = true;
            hasFinishedAlternateAttackTurn = true;
            //ColorDebugLog.Log("Finishing Trun" + gameObject.name, KnownColor.Yellow);
            yield break;
        }
        private void FixedUpdate()
        {
            subComponentsController.OnFixedUpdate();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isDead)
            {
                return;
            }

            if (other.gameObject.layer == LayerMask.NameToLayer("Projectile"))
            {
                subComponentMediator.Notify_TriggerCheckVfx(this, other);
                return;
            }

            if (other.gameObject.layer == LayerMask.NameToLayer("DeathZone"))
            {
                subComponentMediator.Notify_DeathZoneContact(other);
                return;
            }
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

            //TEST WEAPON
            //TODO: implement properly
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

        protected override void Dispose(bool disposing)
        {
            GameLoader.Instance.gameLogic_UIManager.RemoveHealthBar(this);
            base.Dispose(disposing);
        }

        protected override void ReturnToPool()
        {
            if (!GameLoader.Instance.poolManager.characterPoolDictionary[characterSettings.characterType].Contains(this))
            {
                GameLoader.Instance.poolManager.AddObject(characterSettings.characterType, GameLoader.Instance.poolManager.characterPoolDictionary, this);
            }
        }
    }
}