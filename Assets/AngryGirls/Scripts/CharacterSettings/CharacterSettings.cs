using System;
using UnityEngine;

namespace Angry_Girls
{
    public enum UnitType
    {
        Ground, //Damage till grounded.
        AirToGround, //Damage in air and then fall down
        Air,
    }

    [CreateAssetMenu(fileName = "Settings", menuName = "Angry_Girls/CharacterAbilities/CharacterAbilities")]
    public class CharacterSettings: ScriptableObject
    {
        [Header("Setup")]
        [Space(10)]
        [Header("Speed and Force")]
        public Vector3 atHittingObstacle_Speed = new Vector3(0, -2, 0);        
        public Vector3 airbonedAttackMovementSpeed = new Vector3(0, -2, 0);
        public Vector3 groundAttackMovementSpeed = new Vector3(0, 0, 0);
        public Vector3 landingMovementSpeed = new Vector3(0, 0, 0);
        [Space(5)]
        public Vector3 airbonedAttackMovementForce = new Vector3(0, 0, 0);
        [Space(5)]
        [Header("Attack")]
        public float attackDamage = 25f;
        [Space(5)]
        [Header("AttackTime")]
        public float attackTimeDuration = 3f;
        public bool useAnimationNormalizedTimeDuration = false;
        [Space(5)]
        [Header("Unit type")]
        public UnitType unitType;

        [Header("BoxColliders")]
        public BoxColliderUpdater_Container boxcolliderContainer;

        [Header("Animations and durations")]
        public CharAnimationData<GroundIdle_States> idle_State;
        public CharAnimationData<AirbonedFlying_States> airbonedFlying_States;
        public CharAnimationData<AirbonedAttack_States> airbonedAttack_State;
        public float timesToRepeat_AirbonedAttack_State = 1f;
        public CharAnimationData<GroundedAttack_States> groundedAttack_State;
        public CharAnimationData<Landing_States> landing_State;
    }

    [System.Serializable]
    public class CharAnimationData<T> where T : Enum
    {
        public T animation;
        public float transitionDuration;
    }
}