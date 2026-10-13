using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

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
        [field: SerializeField] public bool IsAttacking { get; private set; }

        //public bool isLanding = false;
        public bool isDead = false;
        public bool isBehaviorAlternate = true;
        public bool canCheckGlobalBehavior = false;

        //public bool unitGotHit = false;
        public bool hasBeenLaunched = false;
        public bool canUseAbility = false;
        public bool hasUsedAbility = false;
        public bool hasFinishedLaunchingTurn = false;
        public bool hasFinishedAlternateAttackTurn = true;

        public Action<ProjectileConfig, InteractionData> UnitGotHit;
        public Action<ProjectileConfig, InteractionData> UnitGotKilled;
        public Action UnitHasTouchedDeathZone;
        public Action UnitHasBeenLaunched;
        public Action UnitPerformedAttack;
        public Action UnitCallsForStopAttack;
        public Action UnitHasPerformedLanding;
        public Action UnitIsAirboned;
        public Action UnitHasFinishedLanding;
        public Action UnitHasFinishedHitReaction;


        [field: SerializeField] public CharacterHealth Health { get; private set; }
        [field: SerializeField] public CharacterMovement CharacterMovement { get; private set; }
        [field: SerializeField] public Ragdoll Ragdoll { get; private set; }

        public CharacterProfile profile;
        private CharacteProfileInfo _characterProfileInfo;
        public CharacterSettings CharacterSettings => profile.CharacterSettings;
        private SubComponentsController _subComponentsController;

        public BoxCollider boxCollider;
        public Animator animator;

        public AttackAbility attackAbility;

        public PlayerOrAi playerOrAi;

        public Transform projectileSpawnTransform;
        public Transform wingsTransform;
        public Color vfxColor;
        public Transform weaponHolder;

        [SerializeField] public List<GameObject> detectedGroundObject = new();

        [SerializeField] private PhysicsMaterial _noBounceMaterial;

        private VFXManager _vFXManager;
        private AudioManager _audioManager;
        private CameraManager _cameraManager;
        private AttackAbilityManager _attackAbilityManager;
        private TurnManager _turnManager;

        #region init

        public void InitializeCharacter()
        {
            //coreRefs
            _vFXManager = CoreManager.Instance.VFXManager;
            _audioManager = CoreManager.Instance.AudioManager;
            _cameraManager = GameplayCoreManager.Instance.CameraManager;
            _attackAbilityManager = GameplayCoreManager.Instance.AttackAbilityManager;
            _turnManager = GameplayCoreManager.Instance.TurnManager;

#if UNITY_EDITOR
            UnitCallsForStopAttack += () => { Debug.Log($"{this.name} UnitCallsForStopAttack invoking"); };
            UnitIsAirboned += () => { Debug.Log($"{this.name} UnitIsAirboned invoking"); };
            UnitHasPerformedLanding += () => { Debug.Log($"{this.name} UnitHasPerformedLanding invoking"); };
            UnitHasFinishedLanding += () => { Debug.Log($"{this.name} UnitHasFinishedLanding invoking"); };
            UnitPerformedAttack += () => { Debug.Log($"{this.name} UnitPerformedAttack invoking"); };
#endif

            //subscribe
            UnitPerformedAttack += SetAttackBoolTrue;
            UnitCallsForStopAttack += SetAttackBoolfalse;
            UnitGotHit += CheckAndApplyIncomingDamage;
            UnitGotKilled += ApplyDeath;
            UnitHasTouchedDeathZone += ApplyDeathByDeadZone;

            //components
            Ragdoll = GetComponent<Ragdoll>();
            Health = GetComponent<CharacterHealth>();
            CharacterMovement = GetComponent<CharacterMovement>();
            animator = GetComponent<Animator>();
            boxCollider = GetComponent<BoxCollider>();
            _subComponentsController = GetComponentInChildren<SubComponentsController>();
            _subComponentsController.OnAwake();

            Health.Initialize(profile.GetCurrentStats.health);
            _characterProfileInfo = GetComponent<CharacteProfileInfo>();
            _characterProfileInfo.InitAndRun(profile);


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

            attackAbility = _attackAbilityManager.GetAbility(CharacterSettings.attackType);
        }
        private void SetAttackBoolTrue() => IsAttacking = true;
        public void SetAttackBoolfalse() => IsAttacking = false;

        private void OnDestroy()
        {
            //unsubscribe bleat!!!
            UnitGotHit -= CheckAndApplyIncomingDamage;
            UnitGotKilled -= ApplyDeath;
            UnitHasTouchedDeathZone -= ApplyDeathByDeadZone;
        }

        private void Start()
        {

            _subComponentsController.OnStart();
        }

        private void FixedUpdate()
        {
            _subComponentsController.OnFixedUpdate();
        }
        private void Update() => _subComponentsController.OnUpdate();
        private void LateUpdate() => _subComponentsController.OnLateUpdate();
        #endregion

        #region private

        private void CheckAndApplyIncomingDamage(ProjectileConfig projectileConfig, InteractionData interactionData)
        {
            if (isDead || projectileConfig == null || projectileConfig.damage == 0)
                return;

            Health.ApplyDamage(projectileConfig.damage);

            _vFXManager.ShowDamageNumbers(interactionData.targetCollider, projectileConfig.VFXConfig.originator, projectileConfig.damage);
            _audioManager.PlayRandomSound(AudioSourceType.CharacterHit);


            if (Health.CurrentHealth <= 0)
            {
                // Use the existing death event so the animation state machine can enter its terminal phase.
                UnitGotKilled?.Invoke(projectileConfig, interactionData);
            }
        }
private void ApplyKnockback(ProjectileConfig projectileConfig, Vector3 forceDirection, Vector3 hitPoint)
        {
            var force = forceDirection * projectileConfig.deadbodyForceMultiplier;

            Ragdoll.ProcessRagdoll(
                rigidbody: CharacterMovement.Rigidbody,
                forceValue: force,
                forceApplyPosition: hitPoint,
                forceMode: projectileConfig.deadbodyForceMode);
        }

private void ApplyDeath(ProjectileConfig projectileConfig, InteractionData interactionData)
        {
            if (isDead)
                return;

            // Resolve hit data BEFORE SetDeathParams: it disables the body collider used by hit queries
            var hitPoint = GetHitPoint(interactionData);
            var forceDirection = GetProjectileForceDirection(projectileConfig, interactionData);

            SetDeathParams();

            _cameraManager?.StopCameraFollowForRigidBody();

            ApplyKnockback(projectileConfig, forceDirection, hitPoint);
        }

        private void ApplyDeathByDeadZone()
        {
            if (isDead)
                return;

            Health.ApplyDamage(Health.CurrentHealth);
            _cameraManager?.StopCameraFollowForRigidBody();

            SetDeathParams();

            Ragdoll.ProcessRagdoll(
                rigidbody: CharacterMovement.Rigidbody,
                forceValue: new Vector3(10f, 10f, 0f),
                forceApplyPosition: CharacterMovement.Rigidbody.worldCenterOfMass,
                forceMode: ForceMode.VelocityChange);
        }

        /// <summary>
        /// Configures character for death state, enabling ragdoll physics
        /// </summary>
        private void SetDeathParams()
        {
            isDead = true;
            FinishTurn();

            CharacterMovement.Rigidbody.constraints =
                (CharacterMovement.Rigidbody.constraints &
                 ~(RigidbodyConstraints.FreezePositionX |
                   RigidbodyConstraints.FreezePositionY |
                   RigidbodyConstraints.FreezeRotationZ)) |
                RigidbodyConstraints.FreezePositionZ |
                RigidbodyConstraints.FreezeRotationX |
                RigidbodyConstraints.FreezeRotationY;
            CharacterMovement.Rigidbody.useGravity = true;
            CharacterMovement.Rigidbody.isKinematic = false;
            CharacterMovement.Rigidbody.interpolation = RigidbodyInterpolation.None;
            CharacterMovement.Rigidbody.detectCollisions = false;
            boxCollider.enabled = false;

            // Some character prefabs do not have a wingsTransform assigned.
            // Death processing must continue so the ragdoll can still be activated.
            if (wingsTransform != null)
            {
                var wingsAnimator = wingsTransform.GetComponentInChildren<Animator>();
                if (wingsAnimator != null)
                    wingsAnimator.enabled = false;
            }

            gameObject.layer = LayerMask.NameToLayer("DeadBody");
        }

private Vector3 GetProjectileForceDirection(ProjectileConfig projectileConfig, InteractionData interactionData)
        {
            var impact = interactionData.impact;

            // Hit sources that did not capture an impact: read the projectile directly while it is still alive
            if (!impact.IsValid
                && interactionData.source != null
                && interactionData.source.activeInHierarchy
                && interactionData.source.TryGetComponent<Projectile>(out var projectile))
            {
                impact = projectile.CaptureImpact();
            }

            var bodyCenter = GetBodyCenter();
            var originator = projectileConfig.VFXConfig?.originator;
            var originatorPosition = originator != null ? originator.transform.position : bodyCenter - Vector3.right;

            return DeadbodyForceCalculator.CalculateDirection(
                in impact,
                bodyCenter,
                originatorPosition,
                projectileConfig.deadbodyDirectionMode,
                projectileConfig.deadbodyVerticalBias);
        }

private Vector3 GetHitPoint(InteractionData interactionData)
        {
            var bodyCenter = GetBodyCenter();
            Vector3 point;

            if (interactionData.collision != null && interactionData.collision.contactCount > 0)
            {
                point = interactionData.collision.GetContact(0).point;
            }
            else if (interactionData.impact.IsValid && interactionData.targetCollider != null)
            {
                point = interactionData.targetCollider.ClosestPoint(interactionData.impact.Position);
            }
            else
            {
                point = bodyCenter;
            }

            // Keep the hit point on the gameplay plane of the body
            point.z = bodyCenter.z;
            return point;
        }

private Vector3 GetBodyCenter()
        {
            return boxCollider != null ? boxCollider.bounds.center : transform.position;
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
        public async void FinishTurn()
        {
            canUseAbility = false;
            if (CharacterSettings.unitType != UnitType.Air)
            {
                // Create a token that will cancel after 4 seconds
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(4)))
                {
                    try
                    {
                        // Wait until it lands, but no more than 4 seconds
                        await UniTask.WaitWhile(() => (!CharacterMovement.IsGrounded),
                        cancellationToken: cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        // Timeout - 4 seconds have passed, and the unit is still in the air
                        Debug.LogWarning("Wait grounded timeout after 4 seconds");
                    }
                }
            }

            //general delay
            var delay = (int)(_turnManager.TimeToWaitAfterUnitFinishedAttack * 1000);
            await UniTask.Delay(delay);

            hasFinishedLaunchingTurn = true;
            hasFinishedAlternateAttackTurn = true;
        }

        public AnimatorStateInfo GetAnimatorStateInfo()
        {
            return animator.GetCurrentAnimatorStateInfo(0);
        }

        ///// <summary>
        ///// Checks if character can finish attack based on ground state and turn
        ///// </summary>
        ///// <returns>True if attack can be finished</returns>
        //public bool CheckAttackFinishCondition()
        //{
        //    if (isDead) return false;

        //    var currentAttacker = GameplayCoreManager.Instance.GameplayCharactersManager.CurrentlyAttackingUnit;
        //    return CharacterMovement.IsGrounded && currentAttacker == this;
        //}

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

        /// <summary>
        /// Gets the hash of currently playing animation
        /// </summary>
        /// <returns>Current animation state name</returns>
        public int GetCurrentAnimationHash()
        {
            return animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
        }

        public bool IsInIdleState()
        {
            if (StatesContainer.IdleDictionary.ContainsValue(animator.GetCurrentAnimatorStateInfo(0).shortNameHash))
            {
                return true;
            }

            return false;
        }

        ///// <summary>
        ///// Checks if animation is ending based on normalized time
        ///// </summary>
        //public bool IsAnimationEnding<T>(int stateHash, SerializedDictionary<T, int> dict) where T : Enum
        //{
        //    if (dict.ContainsValue(stateHash))
        //    {
        //        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// Checks if animation is near its end, properly handling looped animations
        ///// </summary>
        ///// <param name="stateHash">Hash of animation to check</param>
        ///// <param name="dict">Dictionary containing animation hashes</param>
        ///// <param name="threshold">Normalized time threshold (0-1)</param>
        ///// <returns>True if animation is near its end</returns>
        //public bool IsAnimationNearEnd<T>(int stateHash, SerializedDictionary<T, int> dict, float threshold = 0.9f) where T : Enum
        //{
        //    // Check if this animation is in our dictionary
        //    if (!dict.ContainsValue(stateHash))
        //        return false;

        //    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        //    // Get normalized time within current loop (0-1)
        //    float normalizedTime = stateInfo.normalizedTime % 1f;

        //    // For looped animations, check if we're near the end of current loop
        //    return normalizedTime >= threshold;
        //}

        public bool IsAnimationEnding(Animator animator, float threshold)
        {
            AnimatorStateInfo info;
            if (animator.IsInTransition(0))
            {
                info = animator.GetNextAnimatorStateInfo(0);
            }
            else
            {
                info = animator.GetCurrentAnimatorStateInfo(0);
            }

            var stateName = StatesContainer.GetStateNameByHash(info.shortNameHash);

            return info.normalizedTime >= threshold;
        }

        ///// <summary>
        ///// Checks if current animation (regardless of type) is near its end
        ///// </summary>
        //public bool IsAnyAnimationNearEnd(float threshold = 0.9f)
        //{

        //    var current = animator.GetCurrentAnimatorStateInfo(0);
        //    if (IsStateNearEnd(current, threshold)) return true;

        //    if (animator.IsInTransition(0))
        //    {
        //        var next = animator.GetNextAnimatorStateInfo(0);
        //        if (IsStateNearEnd(next, threshold)) return true;
        //    }
        //    return false;
        //}

        //private bool IsStateNearEnd(AnimatorStateInfo stateInfo, float threshold)
        //{
        //    float cycleTime = stateInfo.normalizedTime/* % 1f*/;
        //    return cycleTime >= threshold;
        //}

        /// <summary>
        /// Gets the current layer of the character's gameObject
        /// </summary>
        /// <returns>Layer name as string</returns>
        public string GetCurrentLayerName()
        {
            return LayerMask.LayerToName(gameObject.layer);
        }

        #endregion
    }
}
