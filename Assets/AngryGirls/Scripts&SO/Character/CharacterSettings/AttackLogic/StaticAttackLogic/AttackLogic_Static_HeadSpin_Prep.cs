using DG.Tweening;
using System;
using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_Static_HeadSpin_Prep : AttackAbilityLogic
    {
        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            //Move character when casting ability
            control.rigidBody.velocity = control.characterSettings.staticAttackAbility.attackPrepMovementSpeed;
            control.rigidBody.AddForce(control.characterSettings.staticAttackAbility.attackPrepMovementForce);
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            //if (stateInfo.normalizedTime >= 0.9)
            //{
            //TODO: fuck...
            //    control.subComponentMediator.TEMP_SetHeadSpinState();
            //}
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