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
        public Vector3 landingAttackMovementSpeed = new Vector3(0, 0, 0);

        [Header("BoxColliders")]
        public BoxColliderUpdater_Container boxcolliderContainer;

        [Header("Animations")]
        public GroundIdle_States idle_State;
        public AirbonedIdle_States airbonedIdle_State;
        public AirbonedAttack_States airbonedAttack_State;
        public GroundedAttack_States groundedAttack_State;
        public Landing_States landing_State;
    }
}