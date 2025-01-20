using DG.Tweening;
using System;
using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_AlternateAttack_HeadSpin : AttackAbilityLogic
    {
        private float _impulseY = 7f;
        private float _impulseZ = 5f;
        private Vector3 _finalProjectileRotation = new Vector3(75f, 0, 0);
        private bool _haveShootedFirstTime = false;


       private Vector3[] _firstShoot_ProjectileAngles = {
                  new Vector3(170f,0,0),
                  new Vector3(200f,0,0),
                  new Vector3(230f,0,0),
                  new Vector3(170f,-180f,0),
                  new Vector3(200f,-180f,0),
                  new Vector3(230f,-180f,0)
            };

        private Vector3[] _secondShoot_ProjectileAngles = {
                  new Vector3(-205f,0,0),
                  new Vector3(-225f,0,0),
                  new Vector3(-270f,0,0),
                  new Vector3(-305f,0,0),
                  new Vector3(-325f,0,0),
            };

        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {

            control.isAttacking = true;

            //Move character when casting ability
            control.rigidBody.velocity = control.characterSettings.AttackAbility_Alternate.attackMovementSpeed;
            control.rigidBody.velocity = control.characterSettings.AttackAbility_Alternate.attackMovementSpeed;
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.CheckAttackFinishCondition();

            if (control.rigidBody.velocity.y <= - 0.2f && !_haveShootedFirstTime)            
            {
                //Second cast, second character move
                control.rigidBody.velocity = control.characterSettings.AttackAbility_Alternate.attackMovementSpeed;
                ProcessFireballs(control, _firstShoot_ProjectileAngles);
                _haveShootedFirstTime = true;
                control.isAttacking = false;
            }
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.rigidBody.velocity = control.characterSettings.AttackAbility_Alternate.attackMovementSpeed;
            ProcessFireballs(control, _secondShoot_ProjectileAngles);
            _haveShootedFirstTime = false;
        }

        private void ProcessFireballs(CControl control, Vector3[] angles, float moveDuration = 1.5f)
        {
            //spawn
            for (var i = 0; i < angles.Length; i++)
            {
                var projectile = GameLoader.Instance.VFXManager.SpawnVFX_AtPosition(
                    vfx_Type: control.characterSettings.AttackAbility_Alternate.AttackVFX.GetComponent<VFX>().GetVFXType(),
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
                projectile.GetComponent<Rigidbody>().AddForce(impulse, ForceMode.Impulse);
                projectile.transform.DORotate(endValue: new Vector3(finalRotationDegree.x, finalRotationDegree.y, finalRotationDegree.y * projectile.transform.forward.z), duration: moveDuration, mode: RotateMode.Fast);

                //init and run
                projectile.GetComponent<VFX>().InitAndRunVFX(
                    timeToLive: control.characterSettings.AttackAbility_Alternate.timeToLive,
                    isTimeToLiveIsNormilizedTime: control.characterSettings.AttackAbility_Alternate.isTimeToLiveIsNormilizedTime,
                    destroyOnCollision: control.characterSettings.AttackAbility_Alternate.destroyOnCollision,
                    destroyOnCharacterCollision: control.characterSettings.AttackAbility_Alternate.destroyOnCharacterCollision,
                    damage: control.characterSettings.AttackAbility_Alternate.attackDamage,
                    enableCollider: control.characterSettings.AttackAbility_Alternate.enableCollider,
                    enableTrigger: control.characterSettings.AttackAbility_Alternate.enableTrigger,
                    owner: control.gameObject
                    );
            }
        }
    }
}