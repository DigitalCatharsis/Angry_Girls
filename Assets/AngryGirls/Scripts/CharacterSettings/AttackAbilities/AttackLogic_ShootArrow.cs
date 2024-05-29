using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_ShootArrow : AttackAbilityLogic
    {
        public float currentAttackTimer;
        public int attacksCount;

        private Vector3 _finalProjectileRotation = new Vector3(45f, 0, 0);

        public override void OnStateEnter(CharacterControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            currentAttackTimer = 0;
            attacksCount = 0;

            control.isAttacking = true;
            control.rigidBody.useGravity = false;
            control.rigidBody.velocity = control.characterSettings.attackPrepAbility.attackPrepMovementSpeed;
            control.rigidBody.AddForce(control.characterSettings.attackPrepAbility.attackPrepMovementForce);

            var projectile = Singleton.Instance.spawnManager.SpawnThing<VFX_Type>(VFX_Type.VFX_TestProjectile, control.transform.position, Quaternion.identity);
            projectile.GetComponent<VFX>().SendProjectile_Fireball__TweenMove(control.subComponentProcessor.attackSystem.projectileSpawnPosition.position, _finalProjectileRotation, control.characterSettings.attackPrepAbility.attackDamage);

        }
        public override void OnStateUpdate(CharacterControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (attacksCount < ((int)stateInfo.normalizedTime))
            {
                attacksCount = (int)stateInfo.normalizedTime;
                var projectile = Singleton.Instance.spawnManager.SpawnThing<VFX_Type>(VFX_Type.VFX_TestProjectile, control.transform.position, Quaternion.identity);
                projectile.GetComponent<VFX>().SendProjectile_Fireball__TweenMove(control.subComponentProcessor.attackSystem.projectileSpawnPosition.position, _finalProjectileRotation, control.characterSettings.attackPrepAbility.attackDamage);
            }

            if (control.characterSettings.attackPrepAbility.useAnimationNormalizedTimeDuration)
            {
                if (stateInfo.normalizedTime >= control.characterSettings.attackPrepAbility.timesToRepeat_AttackPrep_State)
                {
                    control.isAttacking = false;
                    control.subComponentProcessor.launchLogic.hasFinishedTurn = true;
                }
            }
            else
            {
                currentAttackTimer += Time.deltaTime;
                if (currentAttackTimer >= control.characterSettings.attackPrepAbility.attackTimeDuration)
                {
                    control.isAttacking = false;
                    control.subComponentProcessor.launchLogic.hasFinishedTurn = true;
                }
            }
        }

        public override void OnStateExit(CharacterControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.rigidBody.velocity = Vector3.zero;
        }
    }
}