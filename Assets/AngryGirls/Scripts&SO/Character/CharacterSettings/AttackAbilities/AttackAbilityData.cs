using UnityEngine;

namespace Angry_Girls
{
    [CreateAssetMenu(fileName = "Settings", menuName = "Angry_Girls/CharacterSettings/AttackAbilityData")]
    [System.Serializable]
    public class AttackAbilityData : ScriptableObject
    {
        [Header("Setup")]
        [Header("Animation")]
        public CharAnimationData<Attack_States> attack_State;

        [Header("AttackFinish state (if unit is ground type)")]
        public CharAnimationData<AttackFinish_States> attackFininsh_State;

        [Space(5)]
        [Header("Attack")]
        public float attackDamage = 25f;
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

        [Space(5)]
        [Header("Projectile Colliders")]
        public bool enableCollider = false;
        public bool enableTrigger = false;

        [Space(5)]
        [Header("SFX")]
        public AudioSourceType spawnSourceType;
        public int spawnIndex;
        public AudioSourceType destroySourceType;
        public int destoyIndex;
    }
}