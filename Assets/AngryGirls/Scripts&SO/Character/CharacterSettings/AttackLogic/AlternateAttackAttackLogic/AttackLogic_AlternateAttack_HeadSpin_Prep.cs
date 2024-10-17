using DG.Tweening;
using System;
using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_AlternateAttack_HeadSpin_Prep : AttackAbilityLogic
    {
        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            //Move character when casting ability
            control.rigidBody.velocity = control.characterSettings.AttackAbility_Alternate.attackMovementSpeed;
            control.rigidBody.AddForce(control.characterSettings.AttackAbility_Alternate.attackMovementForce);
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (control.rigidBody.velocity.y <= 0.0001)
            {
                //TODO: fuck...
                control.subComponentMediator.TEMP_SetHeadSpinState();
            }
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.rigidBody.velocity = Vector3.zero;
            control.rigidBody.useGravity = true;
        }
    }
}