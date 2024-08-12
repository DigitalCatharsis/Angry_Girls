using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Angry_Girls
{
    public enum UnitType
    {
        Ground, //Damage till grounded.
        AirToGround, //Damage in air and then fall down
        Air,
    }

    [CreateAssetMenu(fileName = "Settings", menuName = "Angry_Girls/CharacterSettings/CharacterSettings")]
    public class CharacterSettings : ScriptableObject
    {
        [Header("Debug")]
        [SerializeField] private bool _notifyAboutNONEStates = true;

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
        public CharAnimationData<Idle_States> idle_State;
        public CharAnimationData<AirbonedFlying_States> airbonedFlying_States;
        public CharAnimationData<AttackFinish_States> attackFininsh_State;
        public CharAnimationData<Landing_States> landing_State;

        [Header("Launched Attack Ability")]
        [SerializeReference]
        public AttackAbility launchedAttackPrepAbility;
        [Header("Static Attack Ability")]
        public StaticAttackAbility staticAttackAbility;

        private void  NotifyForNONE_Value<T>(CharAnimationData<T> charAnimationData, CControl control) where T : Enum
        {
            if (charAnimationData.animation.ToString() == "NONE")
            {
                ColorDebugLog.Log(control.name + "'s " + charAnimationData + " is NONE", System.Drawing.KnownColor.Yellow);
            }
        }

        public void CheckForNoneValues(CControl control)
        {
            if (_notifyAboutNONEStates == false)
            {
                return;
            }

            NotifyForNONE_Value(idle_State, control);
            NotifyForNONE_Value(airbonedFlying_States, control);
            NotifyForNONE_Value(attackFininsh_State, control);
            NotifyForNONE_Value(landing_State, control);
        }
    }

    [System.Serializable]
    public class CharAnimationData<T> where T : Enum
    {
        public T animation;
        public float transitionDuration;
    }
}