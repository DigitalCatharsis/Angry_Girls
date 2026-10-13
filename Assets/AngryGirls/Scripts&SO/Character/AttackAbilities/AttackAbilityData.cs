using UnityEngine;

namespace Angry_Girls
{
    public enum AttackUsage {None = 0, Launch = 1, Alternate = 2 }
    public enum AttackType { None = 0, Uppercut = 1, Fireball =2, HeadSpin =3 , SwordSpin = 4 }

    [CreateAssetMenu(fileName = "Settings", menuName = "Angry_Girls/CharacterSettings/AttackAbilityData")]
    [System.Serializable]
    public class AttackAbilityData : ScriptableObject
    {
        [Header("Setup")]

        public AttackType attackType;
        public AttackUsage usage;       



        [Header("Animation")]
        public CharAnimationData<Attack_States> attack_State;
        [Space(5)]
        public float attackTimeDuration = 3f; 
        [Space(5)]
        [Header("Enemy Knockback")]
        public float enemyKnockbackValue = 1f;

        [Space(5)]
        [Header("Character Phyciscs")]
        public Vector3 attackMovementForce = Vector3.zero;

        [Space(15)]
        [Header("Projectile")]
        public VFX_Type vfxType;
        public Color vfxColor = Color.white;
        public bool connectToOwner;
        public bool teamfire;

        [Header("Lifetime")]
        [Space(5)]
        public float timeToLive = 1f;

        [Space(5)]
        [Header("Projectile Phyciscs")]
        public Vector3 projectileMovementSpeed = Vector3.zero;
        public bool destroyOnCollision = false;
        public bool destroyOnCharacterCollision = false;
        public ForceMode deadbodyForceMode;
        public float deadbodyForceMultiplier;
        [Tooltip("ProjectileVelocity: push along projectile flight direction. AwayFromProjectile: push from projectile center to body center.")]
        public DeadbodyForceDirectionMode deadbodyDirectionMode = DeadbodyForceDirectionMode.ProjectileVelocity;
        [Tooltip("Optional extra lift added to the push direction (0 = push exactly relative to the projectile).")]
        [Range(-1f, 1f)]
        public float deadbodyVerticalBias = 0f;


        [Space(5)]
        [Header("Projectile Colliders")]
        public bool enableCollider = false;
        public bool enableTrigger = false;

        [Space(5)]
        [Header("SFX")]
        public AudioClipData[] spawnAudioData;
        public AudioClipData[] destroyAudioData;

        /// <summary>
        /// Reads character movement force on the XY gameplay plane.
        /// Z is accepted as a legacy horizontal value for existing ScriptableObject assets.
        /// </summary>
        public Vector3 GetAttackMovementForceXY(Transform characterTransform)
        {
            float horizontalForce = Mathf.Abs(attackMovementForce.x) > 0.0001f
                ? attackMovementForce.x
                : attackMovementForce.z;

            float facingX = characterTransform != null ? characterTransform.forward.x : 1f;
            if (Mathf.Abs(facingX) <= 0.001f)
                facingX = 1f;

            return new Vector3(
                horizontalForce * Mathf.Sign(facingX),
                attackMovementForce.y,
                0f);
        }

        /// <summary>
        /// Reads horizontal projectile speed, supporting legacy assets that store it in Z.
        /// </summary>
        public float GetProjectileHorizontalSpeed()
        {
            return Mathf.Abs(projectileMovementSpeed.x) > 0.0001f
                ? projectileMovementSpeed.x
                : projectileMovementSpeed.z;
        }
    }
}
