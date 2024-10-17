using DG.Tweening;
using System.Drawing;
using System.Runtime.CompilerServices;
using UnityEditor.Search;
using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_Launch_SendFireball_Front : AttackAbilityLogic
    {
        private float _currentAttackTimer;
        private int _attacksCount;

        private Vector3 _finalProjectileRotation = new Vector3(45f, 0, 0);

        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            _currentAttackTimer = 0;
            _attacksCount = 0;

            control.isAttacking = true;
            control.rigidBody.useGravity = false;

            //stop launched velocity 
            control.rigidBody.velocity = Vector3.zero;

            SendFireball(control, control.projectileSpawnTransform.position, _finalProjectileRotation, control.characterSettings.AttackAbility_Launch.attackDamage);
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (_attacksCount < ((int)stateInfo.normalizedTime))
            {
                _attacksCount = (int)stateInfo.normalizedTime;                

                SendFireball(control, control.projectileSpawnTransform.position, _finalProjectileRotation, control.characterSettings.AttackAbility_Launch.attackDamage);
            }

            if (control.characterSettings.AttackAbility_Launch.useAnimationNormalizedTimeDuration)
            {
                if (stateInfo.normalizedTime >= control.characterSettings.AttackAbility_Launch.timesToRepeat_Attack_State)
                {
                    control.FinishTurn();
                }
            }
            else
            {
                _currentAttackTimer += Time.deltaTime;
                if (_currentAttackTimer >= control.characterSettings.AttackAbility_Launch.attackTimeDuration)
                {
                    control.FinishTurn();
                }
            }
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.rigidBody.velocity = Vector3.zero;
        }

        private void SendFireball(CControl control, Vector3 startPoint, Vector3 finalRotationDegree, float moveDuration = 1.5f)
        {
            //spawn fireball
            var vfx = GameLoader.Instance.VFXManager.SpawnVFX(control, VFX_Type.VFX_FireBall);

            //rotate fireball to proper way
            vfx.transform.forward = control.transform.forward;

            //set impulse
            var impulse = new Vector3(
                0,
                control.characterSettings.AttackAbility_Launch.projectileMovementSpeed.y * vfx.transform.forward.y,
                control.characterSettings.AttackAbility_Launch.projectileMovementSpeed.z * control.transform.forward.z
                );

            //MoveCharacter when cast fireball
            control.rigidBody.AddForce(control.characterSettings.AttackAbility_Launch.attackMovementForce * control.transform.forward.z); //turn it back

            //Move fireball
            vfx.GetComponent<Rigidbody>().AddForce(impulse, ForceMode.VelocityChange);
            vfx.transform.DORotate(endValue: new Vector3(finalRotationDegree.x, finalRotationDegree.y, finalRotationDegree.y * control.transform.forward.z), duration: moveDuration, mode: RotateMode.Fast).SetLink(vfx, LinkBehaviour.PauseOnDisableRestartOnEnable);
        }
    }
}