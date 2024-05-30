using System;
using UnityEngine;

namespace Angry_Girls
{
    [CreateAssetMenu(fileName = "Settings", menuName = "Angry_Girls/CharacterSettings/AttackAbility")]
    public class AttackAbility : ScriptableObject
    {
        [Header("Setup")]
        [Header("Animation")]
        public CharAnimationData<AttackPrep_States> attackPrep_State;
        public float timesToRepeat_AttackPrep_State = 1f;

        [Space(5)]
        [Header("Phyciscs")]
        public Vector3 attackPrepMovementSpeed = new Vector3(0, -2, 0);
        public Vector3 attackPrepMovementForce = new Vector3(0, 0, 0);

        [Space(5)]
        [Header("Projectile")]
        public GameObject attackPrep_Projectile;
        public float attackPrep_Projectile_LifeDuration = 1f;

        [Space(5)]
        [Header("Attack")]
        public float attackDamage = 25f;
        [Space(5)]
        public float attackTimeDuration = 3f;
        public bool useAnimationNormalizedTimeDuration = false;
    }

    public abstract class AttackAbilityLogic
    {
        public virtual void OnStateEnter(CharacterControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
        }

        public virtual void OnStateUpdate(CharacterControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
        }
        public virtual void OnStateExit(CharacterControl control, Animator animator, AnimatorStateInfo stateInfo)
        {

        }
    }
}