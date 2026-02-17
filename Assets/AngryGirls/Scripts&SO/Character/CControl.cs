using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// Represents whether a character is controlled by player or AI
    /// </summary>
    public enum PlayerOrAi
    {
        Player,
        Bot,
    }

    /// <summary>
    /// Main controller class for character units, handling animation, movement, attacks, and state management
    /// </summary>
    public class CControl : PoolObject
    {
        public bool isAttacking = false;
        public bool isDead = false;
        public bool canUseAbility = false;
        public bool isBehaviorAlternate = true;
        public bool canCheckGlobalBehavior = false;
        public bool isLanding = false;

        //public bool unitGotHit = false;
        public bool hasBeenLaunched = false;
        public bool hasUsedAbility = false;
        public bool hasFinishedLaunchingTurn = false;
        public bool hasFinishedAlternateAttackTurn = true;

        public Action<ProjectileConfig, InteractionData> UnitGotHit;
        public Action<ProjectileConfig, InteractionData> UnitGotKilled;
        public Action UnitHasTouchedDeathZone;
        public Action UnitHasBeenLaunched;
        public Action UnitHasFinishedLaunchingTurn;
        public Action UnitHasFinishedAlternateAttackTurn;
        public Action UnitPerformedAttack;
        public Action UnitPerformedAttackFinish;
        public Action UnitCallsForStopAttack;
        public Action UnitCallsForStopAttackfiniss;
        public Action UnitHasPerformedLanding;

        public CharacterHealth Health { get; private set; }
        public CharacterMovement CharacterMovement { get; private set; }
        public Ragdoll Ragdoll { get; private set; }

        public Slider healthSlider;

        public CharacterProfile profile;
        [SerializeField] private CharacteProfileInfo _characterProfileInfo;
        public CharacterSettings CharacterSettings => profile.CharacterSettings;


        public SubComponentMediator subComponentMediator;
        private SubComponentsController subComponentsController;

        public BoxCollider boxCollider;
        public Animator animator;
        public AttackSystem_Data attackSystem_Data;

        public PlayerOrAi playerOrAi;

        public Transform projectileSpawnTransform;
        public Transform wingsTransform;
        public Color vfxColor;
        public Transform weaponHolder;

        public List<GameObject> detectedGroundObject = new();

        [SerializeField] private PhysicMaterial noBounceMaterial;

        private VFXManager _vFXManager;
        private AudioManager _audioManager;
        private CameraManager _cameraManager;

        #region init
        private void Awake()
        {
            //coreRefs
            _vFXManager = CoreManager.Instance.VFXManager;
            _audioManager = CoreManager.Instance.AudioManager;
            _cameraManager = GameplayCoreManager.Instance.CameraManager;

            //subscribe
            UnitGotHit += CheckAndApplyIncomingDamage;
            UnitGotKilled += ApplyDeath;
            UnitHasTouchedDeathZone += ApplyDeathByDeadZone;

            //components
            Ragdoll = GetComponent<Ragdoll>();
            Health = GetComponent<CharacterHealth>();
            CharacterMovement = GetComponent<CharacterMovement>();
            animator = GetComponent<Animator>();
            boxCollider = GetComponent<BoxCollider>();
            subComponentMediator = GetComponentInChildren<SubComponentMediator>();
            subComponentsController = GetComponentInChildren<SubComponentsController>();
            subComponentMediator.OnAwake();
            subComponentsController.OnAwake();

            GameplayCoreManager.Instance.InteractionManager.Register(gameObject, new InteractionConfig
            {
                type = InteractionMemberType.Character,
                ownerGO = gameObject
            });
        }

        private void OnDestroy()
        {
            //unsubscribe bleat!!!
            UnitGotHit -= CheckAndApplyIncomingDamage;
            UnitGotKilled -= ApplyDeath;
            UnitHasTouchedDeathZone -= ApplyDeathByDeadZone;
        }

        private void OnEnable()
        {
            subComponentsController.OnComponentEnable();
        }
        private void Start()
        {
            Health.Initialize(profile.GetCurrentStats.health);
            _characterProfileInfo = GetComponent<CharacteProfileInfo>();
            _characterProfileInfo.InitAndRun(profile);

            subComponentsController.OnStart();

            //TODO: TEMP
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
            AttackLogicContainer.SetCharacterAttackLogic(this);
        }

        private void FixedUpdate()
        {
            subComponentsController.OnFixedUpdate();
        }
        private void Update() => subComponentsController.OnUpdate();
        private void LateUpdate() => subComponentsController.OnLateUpdate();
        #endregion

        #region private

        private void CheckAndApplyIncomingDamage(ProjectileConfig projectileConfig, InteractionData interactionData)
        {
            if (projectileConfig.damage == 0) { return; }
            ;

            Health.ApplyDamage(projectileConfig.damage);

            _vFXManager.ShowDamageNumbers(interactionData.targetCollider, projectileConfig.VFXConfig.originator, projectileConfig.damage);
            _audioManager.PlayRandomSound(AudioSourceType.CharacterHit);


            if (Health.CurrentHealth <= 0)
            {
                ApplyDeath(projectileConfig, interactionData);
            }
        }
        private void ApplyKnockback(ProjectileConfig projectileConfig, InteractionData interactionData)
        {

            var hitPoint = GetHitPoint(interactionData);
            var forceDirection = GetProjectileForceDirection(interactionData);
            var force = forceDirection * Mathf.Abs(projectileConfig.deadbodyForceMultiplier);

            if (projectileConfig.deadbodyForceMultiplier < 0)
                force = -force;

            Ragdoll.ProcessRagdoll(
                rigidbody: CharacterMovement.Rigidbody,
                forceValue: force,
                forceApplyPosition: hitPoint,
                forceMode: projectileConfig.deadbodyForceMode);
        }

        private void ApplyDeath(ProjectileConfig projectileConfig, InteractionData interactionData)
        {
            SetDeathParams();

            _cameraManager.StopCameraFollowForRigidBody();

            ApplyKnockback(projectileConfig, interactionData);
        }

        private void ApplyDeathByDeadZone()
        {
            Health.ApplyDamage(Health.CurrentHealth);
            _cameraManager.StopCameraFollowForRigidBody();

            SetDeathParams();

            Ragdoll.ProcessRagdoll(
                rigidbody: CharacterMovement.Rigidbody,
                forceValue: Vector3.one * 10,
                forceApplyPosition: transform.forward,
                forceMode: ForceMode.VelocityChange);
        }

        /// <summary>
        /// Configures character for death state, enabling ragdoll physics
        /// </summary>
        private void SetDeathParams()
        {
            isDead = true;
            FinishTurn();

            CharacterMovement.Rigidbody.constraints = RigidbodyConstraints.None;
            CharacterMovement.Rigidbody.useGravity = true;
            CharacterMovement.Rigidbody.isKinematic = false;
            CharacterMovement.Rigidbody.interpolation = RigidbodyInterpolation.None;
            CharacterMovement.Rigidbody.detectCollisions = false;
            boxCollider.enabled = false;

            //TODO: temp
            var animator = wingsTransform.GetComponentInChildren<Animator>();
            if (animator != null) animator.enabled = false;

            gameObject.layer = LayerMask.NameToLayer("DeadBody");
        }

        private IEnumerator ExecuteFinishTurnTimer(float timeToCheck = 0)
        {
            float timer = 0;
            while (timer < timeToCheck)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            UnitHasFinishedLaunchingTurn?.Invoke();
            UnitHasFinishedAlternateAttackTurn?.Invoke();
        }

        private Vector3 GetProjectileForceDirection(InteractionData interactionData)
        {
            // 1. Try to get the velocity direction from the Rigidbody
            Rigidbody projectileRb = interactionData.source.GetComponent<Rigidbody>();
            if (projectileRb != null && projectileRb.velocity.sqrMagnitude > 0.1f)
            {
                return projectileRb.velocity.normalized;
            }

            // 2. If there is no Rigidbody, try to determine the direction from rotation
            if (interactionData.source.transform.up.sqrMagnitude > 0.1f)
            {
                return interactionData.source.transform.up;
            }

            // 3. Fallback: direction from the projectile to the target
            Vector3 projectileToTarget = interactionData.target.transform.position - interactionData.source.transform.position;
            if (projectileToTarget.sqrMagnitude > 0.1f)
            {
                return projectileToTarget.normalized;
            }

            // 4. Ultimate fallback: up with a small random 
            return (Vector3.up + UnityEngine.Random.insideUnitSphere * 0.3f).normalized;
        }

        private Vector3 GetHitPoint(InteractionData interactionData)
        {
            // If there was a collision, take the point from contacts[0]
            if (interactionData.physicType == InteractionPhysicType.Collision && interactionData.collision != null)
            {
                return interactionData.collision.contacts[0].point;
            }
            // If there was a trigger, use the closest point on the target collider
            else if (interactionData.targetCollider != null)
            {
                return interactionData.targetCollider.ClosestPoint(interactionData.source.transform.position);
            }
            // Fallback: if there is no data, use the object's center
            else
            {
                return interactionData.target.transform.position;
            }
        }



        /// <summary>
        /// Cleanup when character is disposed
        /// </summary>
        protected override void OnDispose()
        {
            GameplayCoreManager.Instance.InteractionManager.CleanUpForOwner(gameObject);
        }

        /// <summary>
        /// Returns character to object pool for reuse
        /// </summary>
        protected override void ReturnToPool()
        {
            if (!CoreManager.Instance.PoolManager.characterPoolDictionary[CharacterSettings.characterType].Contains(this))
            {
                CoreManager.Instance.PoolManager.AddObject(CharacterSettings.characterType,
                    CoreManager.Instance.PoolManager.characterPoolDictionary, this);
            }
        }

        #endregion
        #region public  
        /// <summary>
        /// Completes the character's current turn and resets attack state
        /// </summary>
        public void FinishTurn()
        {
            canUseAbility = false;
            isAttacking = false;
            StartCoroutine(ExecuteFinishTurnTimer());
        }

        /// <summary>
        /// Checks if character can finish attack based on ground state and turn
        /// </summary>
        /// <returns>True if attack can be finished</returns>
        public bool CheckAttackFinishCondition()
        {
            if (isDead) return false;

            var currentAttacker = GameplayCoreManager.Instance.GameplayCharactersManager.CurrentlyAttackingUnit;
            return CharacterMovement.IsGrounded && currentAttacker == this;
        }

        /// <summary>
        /// Gets the appropriate attack logic based on current game state
        /// </summary>
        /// <returns>Attack ability logic for current phase</returns>
        public AttackAbilityLogic Get_AttackFinish_AttackAbilityLogic()
        {
            return GameplayCoreManager.Instance.PhaseFlowController.CurrentState == GameState.LaunchPhase
                ? attackSystem_Data.launch_AttackFinishLogic
                : attackSystem_Data.alternate_AttackFinishLogic;
        }

        /// <summary>
        /// Gets the attack ability data based on current game state
        /// </summary>
        /// <returns>Attack ability data for current phase</returns>
        public AttackAbilityData Get_AttackAbility()
        {
            return GameplayCoreManager.Instance.PhaseFlowController.CurrentState == GameState.LaunchPhase
                ? CharacterSettings.AttackAbility_Launch
                : CharacterSettings.AttackAbility_Alternate;
        }

        /// <summary>
        /// Checks if another character is an ally
        /// </summary>
        /// <param name="anotherControl">Character to check</param>
        /// <returns>True if characters are on same team</returns>
        public bool IsAlly(CControl anotherControl) => playerOrAi == anotherControl.playerOrAi;

        /// <summary>
        /// Gets the layer mask for character's visual effects based on team
        /// </summary>
        /// <returns>Layer mask for VFX</returns>
        public int GetVfxLayermask()
        {
            return playerOrAi == PlayerOrAi.Player
                ? LayerMask.NameToLayer("Projectile_Character")
                : LayerMask.NameToLayer("Projectile_Bot");
        }

        /// <summary>
        /// Gets the name of currently playing animation
        /// </summary>
        /// <returns>Current animation state name</returns>
        public string GetCurrentAnimationName()
        {
            var currentHash = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            return StatesContainer.GetStateNameByHash(currentHash);
        }

        public bool IsInIdleState()
        {
            if (StatesContainer.IdleDictionary.ContainsValue(animator.GetCurrentAnimatorStateInfo(0).shortNameHash))
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}