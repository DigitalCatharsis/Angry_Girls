using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        public CharacterType characterType;
        public Sprite portrait;

        [Space(5)]
        [Header("Speed and Force")]
        public Vector3 atHittingObstacle_Speed = new Vector3(0, -2, 0);
        public Vector3 groundAttackMovementSpeed = new Vector3(0, 0, 0);
        public Vector3 landingMovementSpeed = new Vector3(0, 0, 0);

        [Header("BoxColliders")]
        public BoxColliderUpdater_Container boxcolliderContainer;

        [Header("Animations")]
        public List<CharAnimationData<Idle_States>> idle_States;
        public CharAnimationData<Falling_States> airbonedFlying_States;
        public CharAnimationData<Landing_States> landing_State;
        [Space(5)]
        public List<CharAnimationData<HitReaction_States>> hitReaction_States;
        public List<CharAnimationData<Death_States>> death_States;

        public bool deathByAnimation = true; //TODO: implement ragdoll

        [Header("Launched Attack Ability")]
        [SerializeReference]
        public AttackAbility AttackAbility_Launch;
        [Header("Alternate Attack Ability")]
        public AttackAbility AttackAbility_Alternate;

        private void  NotifyForNONE_Value<T>(CharAnimationData<T> charAnimationData, CControl control) where T : Enum
        {
            if (charAnimationData.animation.ToString() == "NONE")
            {
                ColorDebugLog.Log(control.name + "'s " + charAnimationData + " is NONE", System.Drawing.KnownColor.Yellow);
            }
        }
        private void  NotifyForNONE_Value<T>(List<CharAnimationData<T>> charAnimationData, CControl control) where T : Enum
        {
            if (charAnimationData.Count == 0)
            {
                ColorDebugLog.Log(control.name + "'s " + charAnimationData + " is empty", System.Drawing.KnownColor.Yellow);
            }

            foreach (var elem in charAnimationData)
            {
                if (elem.animation.ToString() == "NONE")
                {
                    ColorDebugLog.Log(control.name + "'s " + elem + " is NONE", System.Drawing.KnownColor.Yellow);
                }
            }
        }

        public CharAnimationData<T> GetRandomState<T>(List<CharAnimationData<T>> collection) where T : Enum
        {
            var index = UnityEngine.Random.Range(0, collection.Count);
            return collection[index];
        }

        public void CheckForNoneValues(CControl control)
        {
            if (_notifyAboutNONEStates == false)
            {
                return;
            }

            //debug
            NotifyForNONE_Value(idle_States, control);
            NotifyForNONE_Value(airbonedFlying_States, control);
            NotifyForNONE_Value(landing_State, control);
            NotifyForNONE_Value(hitReaction_States, control);
            NotifyForNONE_Value(death_States, control);
        }
    }

    [System.Serializable]
    public class CharAnimationData<T> where T : Enum
    {
        public T animation;
        public float transitionDuration;
    }
}