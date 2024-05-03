using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Angry_Girls
{
    [CreateAssetMenu(fileName = "Settings", menuName = "Angry_Girls/CharacterAbilities/CharacterAbilities")]
    public class CharacterSettings: ScriptableObject
    {
        [Header("Setup")]
        [Space(10)]
        [Header("Values")]
        public Vector3 atHittingObstacle_Speed = new Vector3(0, -2, 0);        
        public Vector3 airbonedAttackMovementSpeed = new Vector3(0, -2, 0);
        public Vector3 groundAttackMovementSpeed = new Vector3(0, 0, 0);
        public Vector3 landingMovementSpeed = new Vector3(0, 0, 0);
        [Space(5)]
        public float attackDamage = 25f;

        [Header("BoxColliders")]
        public BoxColliderUpdater_Container boxcolliderContainer;

        [Header("Animations")]
        public CharAnimationData<GroundIdle_States> idle_State;
        public CharAnimationData<AirbonedFlying_States> airbonedFlying_States;
        public CharAnimationData<AirbonedAttack_States> airbonedAttack_State;
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