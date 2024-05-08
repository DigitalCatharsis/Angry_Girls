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

    [CreateAssetMenu(fileName = "Settings", menuName = "Angry_Girls/CharacterSettings/CharacterAbilities")]
    public class CharacterSettings: ScriptableObject
    {
        [Header("Setup")]
        [Space(10)]
        [Header("Unit type")]
        public UnitType unitType;

        [Space(5)]
        [Header("Speed and Force")]
        public Vector3 atHittingObstacle_Speed = new Vector3(0, -2, 0);
        public Vector3 groundAttackMovementSpeed = new Vector3(0, 0, 0);
        public Vector3 landingMovementSpeed = new Vector3(0, 0, 0);

        [Header("BoxColliders")]
        public BoxColliderUpdater_Container boxcolliderContainer;

        [Header("Animations")]
        public CharAnimationData<GroundIdle_States> idle_State;
        public CharAnimationData<AirbonedFlying_States> airbonedFlying_States;
        public CharAnimationData<AttackFinish_States> attackFininsh_State;
        public CharAnimationData<Landing_States> landing_State;

        [Header("Attack Ability")]
        public AttackAbility attackPrepAbility;

        private void Awake()
        {
            
        }
    }

    [System.Serializable]
    public class CharAnimationData<T> where T : Enum
    {
        public T animation;
        public float transitionDuration;
    }
}