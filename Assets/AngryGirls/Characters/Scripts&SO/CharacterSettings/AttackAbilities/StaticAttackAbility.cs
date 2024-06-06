using System;
using UnityEngine;

namespace Angry_Girls
{
    [CreateAssetMenu(fileName = "Settings", menuName = "Angry_Girls/CharacterSettings/StaticAttackAbility")]
    public class StaticAttackAbility : ScriptableObject
    {
        [Header("Setup")]
        [Header("Animation")]
        public CharAnimationData<StaticAttack_States> staticAttack_State;
        public float timesToRepeat_AttackState = 1f;

        [Space(5)]
        [Header("Attack")]
        public float attackDamage = 25f;
        [Space(5)]
        public float attackTimeDuration = 3f;
        public bool useAnimationNormalizedTimeDuration = false;

        [Space(15)]
        [Header("Projectile")]
        [Space(5)]
        public GameObject attack_Projectile;
        public float attack_Projectile_LifeDuration = 1f;
        [Space(5)]
        [Header("Projectile Phyciscs")]
        public Vector3 attackMovementSpeed = new Vector3(0, -2, 0);
        public Vector3 attackMovementForce = new Vector3(0, 0, 0);
    }
}