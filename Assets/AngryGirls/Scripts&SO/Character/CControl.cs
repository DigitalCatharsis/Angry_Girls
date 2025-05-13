using System.Collections;
using System.Collections.Generic;
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

        public Slider healthSlider;

        public bool isLanding = false;
        public bool isAttacking = false;
        public bool isDead = false;
        public bool unitGotHit = false;
        public bool hasUsedAbility = false;
        public bool canUseAbility = false;
        public bool hasBeenLaunched = false;
        public bool hasFinishedLaunchingTurn = false;
        public bool hasFinishedAlternateAttackTurn = true;

        public bool unitBehaviorIsAlternate = true;
        public bool checkGlobalBehavior = false;

        public SubComponentMediator subComponentMediator;
        private SubComponentsController subComponentsController;

        public BoxCollider boxCollider;
        public Animator animator;
        public AttackSystem_Data attackSystem_Data;

        public PlayerOrAi playerOrAi;
        public CharacterSettings characterSettings;

        public Transform projectileSpawnTransform;
        public Transform wingsTransform;
        public UnityEngine.Color vfxColor;
        public Transform weaponHolder;

        public List<GameObject> detectedGroundObject = new();

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

        private void OnEnable()
        {
            if (playerOrAi == PlayerOrAi.Character)
                GameLoader.Instance.characterManager.playableCharacters.Add(this);
            else
                GameLoader.Instance.characterManager.enemyCharacters.Add(this);

            subComponentsController.OnComponentEnable();
            GameLoader.Instance.attackLogicContainer.SetCharacterAttackLogic(this);
            GameLoader.Instance.gameLogic_UIManager.CreateHealthBar(this);
        }

        private void Start()
        {
            subComponentsController.OnStart();

            if (weaponHolder != null)
            {
                var weaponPrefab = Resources.Load("DefinetlyNotAWeapon") as GameObject;
                var weapon = Instantiate(weaponPrefab, weaponHolder.transform.position, Quaternion.identity);

                weapon.layer = this.gameObject.layer;
                foreach (Transform child in weapon.transform)
                {
                    child.gameObject.layer = this.gameObject.layer;
                }

                weapon.transform.SetParent(weaponHolder);
                weapon.transform.localScale = weaponHolder.localScale;
                weapon.transform.position = weaponHolder.position;
                weapon.transform.rotation = weaponHolder.rotation;
            }
        }

        public void SetDeathParams()
        {
            isDead = true;
            FinishTurn(0);

            CharacterMovement.Rigidbody.constraints = RigidbodyConstraints.None;
            CharacterMovement.Rigidbody.useGravity = true;
            CharacterMovement.Rigidbody.isKinematic = false;
            CharacterMovement.Rigidbody.interpolation = RigidbodyInterpolation.None;
            CharacterMovement.Rigidbody.detectCollisions = false;
            boxCollider.enabled = false;

            var animator = wingsTransform.GetComponentInChildren<Animator>();
            if (animator != null) animator.enabled = false;

            gameObject.layer = LayerMask.NameToLayer("DeadBody");
        }

        public void FinishTurn(float delay = 3f)
        {
            canUseAbility = false;
            isAttacking = false;
            StopCoroutine("ExecuteFinishTurnTimer");
            StartCoroutine(ExecuteFinishTurnTimer(delay));
        }

        private IEnumerator ExecuteFinishTurnTimer(float timeToCheck)
        {
            float timer = 0;
            while (timer < timeToCheck)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            hasFinishedLaunchingTurn = true;
            hasFinishedAlternateAttackTurn = true;
        }

        public bool CheckAttackFinishCondition()
        {
            if (isDead) return false;
            if (unitGotHit) return true;
            if (characterSettings.unitType == UnitType.Air && hasUsedAbility) return true;
            if (GameLoader.Instance.turnManager == null) return false;

            if (GameLoader.Instance.gameFlowController.CurrentState == GameState.LaunchPhase)
            {
                if (hasFinishedAlternateAttackTurn) return false;
                if (canUseAbility) return false;
            }

            if (CharacterMovement.IsGrounded) return true;
            return false;
        }

        public AttackAbilityLogic Get_AttackFinish_AttackAbilityLogic()
        {
            return GameLoader.Instance.gameFlowController.CurrentState == GameState.LaunchPhase
                ? attackSystem_Data.launch_AttackFinishLogic
                : attackSystem_Data.alternate_AttackFinishLogic;
        }

        public AttackAbilityData Get_AttackAbility()
        {
            return GameLoader.Instance.gameFlowController.CurrentState == GameState.LaunchPhase
                ? characterSettings.AttackAbility_Launch
                : characterSettings.AttackAbility_Alternate;
        }

        private void FixedUpdate() => subComponentsController.OnFixedUpdate();
        private void Update() => subComponentsController.OnUpdate();
        private void LateUpdate() => subComponentsController.OnLateUpdate();

        protected override void OnDispose()
        {
            GameLoader.Instance.interactionManager.CleanUpForOwner(gameObject);
            GameLoader.Instance.gameLogic_UIManager.RemoveHealthBar(this);
        }

        protected override void ReturnToPool()
        {
            if (!GameLoader.Instance.poolManager.characterPoolDictionary[characterSettings.characterType].Contains(this))
            {
                GameLoader.Instance.poolManager.AddObject(characterSettings.characterType,
                    GameLoader.Instance.poolManager.characterPoolDictionary, this);
            }
        }

        public bool IsAlly(CControl anotherControl) => playerOrAi == anotherControl.playerOrAi;

        public int GetVfxLayermask()
        {
            return playerOrAi == PlayerOrAi.Character
                ? LayerMask.NameToLayer("Projectile_Character")
                : LayerMask.NameToLayer("Projectile_Bot");
        }

        public string GetCurrentAnimationName()
        {
            var currentHash = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            return GameLoader.Instance.statesContainer.GetStateNameByHash(currentHash);
        }

    }
}