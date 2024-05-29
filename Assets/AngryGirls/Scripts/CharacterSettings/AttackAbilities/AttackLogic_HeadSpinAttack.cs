using System;
using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_HeadSpinAttack : AttackAbilityLogic
    {
        private float _impulseY = 7f;
        private float _impulseZ = 5f;
        private Vector3 _finalProjectileRotation = new Vector3(75f, 0, 0);
        public float currentAttackTimer;
        public int attacksCount;
        public override void OnStateEnter(CharacterControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            currentAttackTimer = 0;
            attacksCount = 0;

            control.isAttacking = true;
            control.rigidBody.velocity = control.characterSettings.attackPrepAbility.attackPrepMovementSpeed;
            control.rigidBody.AddForce(control.characterSettings.attackPrepAbility.attackPrepMovementForce);

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

        public override void OnStateUpdate(CharacterControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (control.characterSettings.attackPrepAbility.useAnimationNormalizedTimeDuration)
            {
                if (stateInfo.normalizedTime >= control.characterSettings.attackPrepAbility.timesToRepeat_AttackPrep_State)
                {
                    control.isAttacking = false;
                    control.subComponentProcessor.animationProcessor.airToGroundFinishedAbility = true;
                }
            }
            else
            {
                currentAttackTimer += Time.deltaTime;
                if (currentAttackTimer >= control.characterSettings.attackPrepAbility.attackTimeDuration)
                {
                    control.isAttacking = false;
                    control.subComponentProcessor.animationProcessor.airToGroundFinishedAbility = true;
                }
            }
        }

        public override void OnStateExit(CharacterControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            Vector3[] angles = {
                  new Vector3(-205f,0,0),
                  new Vector3(-225f,0,0),
                  new Vector3(-270f,0,0),
                  new Vector3(-305f,0,0),
                  new Vector3(-325f,0,0),
            };

            ProcessFireballs(control, angles);

            control.isAttacking = false;
        }

        private void ProcessFireballs(CharacterControl control, Vector3[] angles)
        {
            for (var i = 0; i < angles.Length; i++)
            {
                //spawn
                var projectile = Singleton.Instance.spawnManager.SpawnThing<VFX_Type>(VFX_Type.VFX_TestProjectile, control.transform.position, Quaternion.Euler(angles[i]));

                //set final rotation
                var rotation = _finalProjectileRotation;
                if (Math.Sign(projectile.transform.forward.z) < 0)
                {
                    rotation.y += 180f;
                }
                
                //add impulse and rotate
                projectile.GetComponent<VFX>().SendProjectile_Fireball(new Vector3(0, _impulseY * projectile.transform.forward.y, _impulseZ * projectile.transform.forward.z), rotation, control.characterSettings.attackPrepAbility.attackDamage);
            }
        }
    }
}