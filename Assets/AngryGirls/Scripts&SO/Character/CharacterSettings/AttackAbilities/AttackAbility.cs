using UnityEngine;

namespace Angry_Girls
{
    [CreateAssetMenu(fileName = "Settings", menuName = "Angry_Girls/CharacterSettings/AttackAbility")]
    [System.Serializable]
    public class AttackAbility : ScriptableObject
    {
        [Header("Setup")]
        [Header("Animation")]
        public CharAnimationData<Attack_States> attack_State;
        //public float timesToRepeat_Attack_State = 0f;
        //public float spawnProjectile_TransitionOffset = 0f;

        [Header("AttackFinish state (if unit is ground type)")]
        public CharAnimationData<AttackFinish_States> attackFininsh_State;

        [Space(5)]
        [Header("Attack")]
        public float attackDamage = 25f;
        [Space(5)]
        public float attackTimeDuration = 3f; [Space(5)]
        [Header("Knockback")]
        public float knockback = 1f;
        //public bool useAnimationNormalizedTimeDuration = false;

        [Space(5)]
        [Header("Character Phyciscs")]
        public Vector3 attackMovementSpeed = new Vector3(0, 0, 0);
        //public Vector3 attackMovementForce = new Vector3(0, 0, 0);

        [Space(15)]
        [Header("Projectile")]
        [Space(5)]
        [Header("VFX")]
        public GameObject AttackVFX;

        [Header("Lifetime")]
        [Space(5)]
        public float timeToLive = 1f;
        public bool isTimeToLiveIsNormilizedTime = false;

        [Space(5)]
        [Header("Projectile Phyciscs")]
        public Vector3 projectileMovementSpeed = new Vector3(0, 0, 0);  //?
        public bool destroyOnCollision = false;
        public bool destroyOnCharacterCollision = false;

        [Space(5)]
        [Header("Projectile Colliders")]
        public bool enableCollider = false;
        public bool enableTrigger = false;
    }
}