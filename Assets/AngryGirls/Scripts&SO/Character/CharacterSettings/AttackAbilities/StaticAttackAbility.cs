using UnityEngine;

namespace Angry_Girls
{
    [CreateAssetMenu(fileName = "Settings", menuName = "Angry_Girls/CharacterSettings/StaticAttackAbility")]
    public class StaticAttackAbility : ScriptableObject
    {
        [Header("Setup")]
        [Header("Animation")]
        public CharAnimationData<StaticAttack_States> staticAttack_State;
        public float timesToRepeat_AttackPrep_State = 1f;

        [Space(5)]
        [Header("Attack")]
        public float attackDamage = 25f;
        [Space(5)]
        public float attackTimeDuration = 3f;
        public bool useAnimationNormalizedTimeDuration = false;

        [Space(5)]
        [Header("Character Phyciscs")]
        public Vector3 attackPrepMovementSpeed = new Vector3(0, 0, 0);
        public Vector3 attackPrepMovementForce = new Vector3(0, 0, 0);

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

        [Space(5)]
        [Header("Projectile Colliders")]
        public bool enableCollider = false;
        public bool enableTrigger = false;
    }
}