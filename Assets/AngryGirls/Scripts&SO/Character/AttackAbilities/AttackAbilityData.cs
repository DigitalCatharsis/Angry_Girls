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

        [Space(5)]
        [Header("Projectile Colliders")]
        public bool enableCollider = false;
        public bool enableTrigger = false;

        [Space(5)]
        [Header("SFX")]
        public AudioClipData[] spawnAudioData;
        public AudioClipData[] destroyAudioData;
    }
}