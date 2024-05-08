using UnityEditor;
using UnityEditor.ShaderGraph;
using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_HeadSpinAttack : AttackAbilityLogic
    {
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

            for (var i = 0; i < angles.Length; i++)
            {
                var projectile = Singleton.Instance.spawnManager.SpawnThing<VFX_Type>(VFX_Type.VFX_TestProjectile, control.transform.position, Quaternion.Euler(angles[i]));
                projectile.GetComponent<VFX>().SendProjectile_Fireball(control.subComponentProcessor.attackSystem.projectileSpawnPosition.position, new Vector3(90f,0,0));
            }
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

            Singleton.Instance.spawnManager.SpawnThing<VFX_Type>(VFX_Type.VFX_TestProjectile, control.transform.position, Quaternion.identity);
            Singleton.Instance.spawnManager.SpawnThing<VFX_Type>(VFX_Type.VFX_TestProjectile, control.transform.position, Quaternion.identity);
            Singleton.Instance.spawnManager.SpawnThing<VFX_Type>(VFX_Type.VFX_TestProjectile, control.transform.position, Quaternion.identity);
            Singleton.Instance.spawnManager.SpawnThing<VFX_Type>(VFX_Type.VFX_TestProjectile, control.transform.position, Quaternion.identity);
            control.isAttacking = false;
        }
    }
}