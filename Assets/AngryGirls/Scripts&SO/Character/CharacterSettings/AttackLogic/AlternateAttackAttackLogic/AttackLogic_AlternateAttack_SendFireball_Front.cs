using DG.Tweening;
using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_AlternateAttack_SendFireball_Front_Alternate : AttackAbilityLogic
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

            SendFireball(control, control.projectileSpawnTransform.position, _finalProjectileRotation, control.characterSettings.AttackAbility_Alternate.attackDamage);

        }
        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (_attacksCount < ((int)stateInfo.normalizedTime))
            {
                _attacksCount = (int)stateInfo.normalizedTime;

                SendFireball(control, control.projectileSpawnTransform.position, _finalProjectileRotation, control.characterSettings.AttackAbility_Alternate.attackDamage);
            }

            if (control.characterSettings.AttackAbility_Alternate.useAnimationNormalizedTimeDuration)
            {
                if (stateInfo.normalizedTime >= control.characterSettings.AttackAbility_Alternate.timesToRepeat_Attack_State)
                {
                    control.FinishTurn();
                    //control.isAttacking = false;
                    //control.hasFinishedAlternateAttackTurn = true;
                }
            }
            else
            {
                _currentAttackTimer += Time.deltaTime;
                if (_currentAttackTimer >= control.characterSettings.AttackAbility_Alternate.attackTimeDuration)
                {
                    control.FinishTurn();
                    //control.isAttacking = false;
                    //control.hasFinishedAlternateAttackTurn = true;
                }
            }
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.rigidBody.velocity = Vector3.zero;
        }

        private void SendFireball(CControl control, Vector3 startPoint, Vector3 finalRotationDegree, float moveDuration = 1.5f)
        {
            var vfx = GameLoader.Instance.VFXManager.SpawnVFX(control, VFX_Type.VFX_FireBall);
            //Debug.Break();
            var impulse = new Vector3(
                0,
                control.characterSettings.AttackAbility_Alternate.projectileMovementSpeed.y * vfx.transform.forward.y,
                control.characterSettings.AttackAbility_Alternate.projectileMovementSpeed.z * control.transform.forward.z
                );

            control.rigidBody.AddForce(control.characterSettings.AttackAbility_Alternate.attackMovementForce); //turn it back
            //vfx.GetComponent<VFX>().InitAndRunVFX(control.characterSettings.AlternateAttackAbility, control.gameObject);
            vfx.GetComponent<Rigidbody>().AddForce(impulse, ForceMode.VelocityChange);
            vfx.transform.DORotate(endValue: new Vector3(finalRotationDegree.x, finalRotationDegree.y, finalRotationDegree.y * control.transform.forward.z), duration: moveDuration, mode: RotateMode.Fast).SetLink(vfx, LinkBehaviour.PauseOnDisableRestartOnEnable);
        }
    }
}