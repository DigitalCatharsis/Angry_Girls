using DG.Tweening;
using System;
using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_Launch_HeadSpinAttack : AttackAbilityLogic
    {
        public AttackLogic_Launch_HeadSpinAttack(AttackAbilityData attackAbilityData) : base(attackAbilityData) { }


        private float _impulseY = 7f;
        private float _impulseZ = 5f;
        private Vector3 _finalProjectileRotation = new Vector3(75f, 0, 0);
        private int _timesToRepeat_Attack_State = 2;
        private bool _haveShootedSecondTime = false;

        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            base.OnStateEnter(control, animator, stateInfo);

            Vector3[] angles = {
                  new Vector3(170f,0,0),
                  new Vector3(200f,0,0),
                  new Vector3(230f,0,0),
                  new Vector3(170f,-180f,0),
                  new Vector3(200f,-180f,0),
                  new Vector3(230f,-180f,0)
            };

            ProcessFireballs(control, angles);
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (!_haveShootedSecondTime && stateInfo.normalizedTime >= _timesToRepeat_Attack_State)
            {
                Vector3[] angles = {
                  new Vector3(-205f,0,0),
                  new Vector3(-225f,0,0),
                  new Vector3(-270f,0,0),
                  new Vector3(-305f,0,0),
                  new Vector3(-325f,0,0),
            };

                //Second cast, second character move
                control.CharacterMovement.ApplyRigidForce(control.characterSettings.AttackAbility_Launch.attackMovementForce, ForceMode.VelocityChange);
                ProcessFireballs(control, angles);
                _haveShootedSecondTime = true;
                control.isAttacking = false;
            }
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            _haveShootedSecondTime = false;
            control.isAttacking = false;
        }

        private void ProcessFireballs(CControl control, Vector3[] angles, float moveDuration = 1.5f)
        {
            //spawn
            for (var i = 0; i < angles.Length; i++)
            {
                var projectile = GameLoader.Instance.VFXManager.SpawnVFX_AtPosition(
                    VFX_Type.VFX_FireBall,
                    control.projectileSpawnTransform.position,
                    Quaternion.Euler(angles[i]));

                //set final rotation value 
                var finalRotationDegree = _finalProjectileRotation;
                if (Math.Sign(projectile.transform.forward.z) < 0)
                {
                    finalRotationDegree.y += 180f;
                }

                var impulse = new Vector3(0, _impulseY * projectile.transform.forward.y, _impulseZ * projectile.transform.forward.z);

                //add impulse and rotate
                projectile.GetComponent<Rigidbody>().AddForce(impulse, ForceMode.VelocityChange);
                projectile.transform.DORotate(endValue: new Vector3(finalRotationDegree.x, finalRotationDegree.y, finalRotationDegree.y * projectile.transform.forward.z), duration: moveDuration, mode: RotateMode.Fast);

                //init and run
                projectile.GetComponent<VFX>().InitAndRunVFX_ByAbility(control.characterSettings.AttackAbility_Launch, control);
            }
        }
    }
}