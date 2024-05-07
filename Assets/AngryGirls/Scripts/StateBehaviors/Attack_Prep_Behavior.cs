using DG.Tweening;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

namespace Angry_Girls
{
    public class Attack_Prep_Behavior : StateMachineBehaviour
    {
        private CharacterControl _control;
        private float _curentAttackTimer;

        private int _attackCount = 0;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_control == null)
            {
                _control = animator.transform.root.GetComponent<CharacterControl>();
            }

            _attackCount = 0;
            _curentAttackTimer = 0f;
            _control.isAttacking = true;

            //Air
            if (_control.characterSettings.unitType == UnitType.Air)
            {
                _control.rigidBody.useGravity = false;
                _control.rigidBody.velocity = _control.characterSettings.airbonedAttackMovementSpeed;
                _control.rigidBody.AddForce(_control.characterSettings.airbonedAttackMovementForce);


                Singleton.Instance.spawnManager.SpawnThing<VFX_Type>(VFX_Type.VFX_TestProjectile, _control.transform.position, Quaternion.identity);
            }

            //AirToGround
            if (_control.characterSettings.unitType == UnitType.AirToGround)
            {
                _control.rigidBody.velocity = _control.characterSettings.airbonedAttackMovementSpeed;
                _control.rigidBody.AddForce(_control.characterSettings.airbonedAttackMovementForce);
            }

            //Ground
            if (_control.characterSettings.unitType == UnitType.Ground)
            {
                _control.rigidBody.velocity = _control.characterSettings.airbonedAttackMovementSpeed;
            }
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //ColorDebugLog.Log(stateInfo.normalizedTime.ToString(), System.Drawing.KnownColor.Azure);
            //Air
            if (_control.characterSettings.unitType == UnitType.Air)
            {
                //stateInfo.
                if (_attackCount < ((int)stateInfo.normalizedTime))
                {
                    //ColorDebugLog.Log(stateInfo.normalizedTime.ToString(), System.Drawing.KnownColor.Aqua);
                    _attackCount = (int)stateInfo.normalizedTime;
                    //ColorDebugLog.Log(_attackCount.ToString(), System.Drawing.KnownColor.Aquamarine);
                    Singleton.Instance.spawnManager.SpawnThing<VFX_Type>(VFX_Type.VFX_TestProjectile, _control.transform.position, Quaternion.identity);
                }

                if (_control.characterSettings.useAnimationNormalizedTimeDuration)
                {
                    if (stateInfo.normalizedTime >= _control.characterSettings.timesToRepeat_AirbonedAttack_State)
                    {
                        _control.isAttacking = false;
                        _control.subComponentProcessor.launchLogic.hasFinishedTurn = true;
                    }
                }
                else
                {
                    _curentAttackTimer += Time.deltaTime;
                    if (_curentAttackTimer >= _control.characterSettings.attackTimeDuration)
                    {
                        _control.isAttacking = false;
                        _control.subComponentProcessor.launchLogic.hasFinishedTurn = true;
                    }
                }
            }
            //AirToGround
            if (_control.characterSettings.unitType == UnitType.AirToGround)
            {
                if (_control.characterSettings.useAnimationNormalizedTimeDuration)
                {
                    if (stateInfo.normalizedTime >= _control.characterSettings.timesToRepeat_AirbonedAttack_State)
                    {
                        _control.isAttacking = false;
                        _control.subComponentProcessor.animationProcessor.airToGroundFinishedAbility = true;
                    }
                }
                else
                {
                    _curentAttackTimer += Time.deltaTime;
                    if (_curentAttackTimer >= _control.characterSettings.attackTimeDuration)
                    {
                        _control.isAttacking = false;
                        _control.subComponentProcessor.animationProcessor.airToGroundFinishedAbility = true;
                    }
                }
            }
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_control.characterSettings.unitType == UnitType.Air)
            {
                _control.rigidBody.velocity = Vector3.zero;
            }
            if (_control.characterSettings.unitType == UnitType.Ground)
            {
                //will go to fall in AnimationProcessor
            }
            if (_control.characterSettings.unitType == UnitType.AirToGround)
            {
                _control.isAttacking = false;
                //will go to fall in AnimationProcessor
            }


        }

        // OnStateMove is called right after Animator.OnAnimatorMove()
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that processes and affects root motion
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK()
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that sets up animation IK (inverse kinematics)
        //}
    }

}