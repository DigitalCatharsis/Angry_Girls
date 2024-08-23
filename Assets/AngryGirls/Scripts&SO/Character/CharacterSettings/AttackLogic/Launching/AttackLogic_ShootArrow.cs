using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_ShootArrow : AttackAbilityLogic
    {
        public float currentAttackTimer;
        public int attacksCount;

        private Vector3 _finalProjectileRotation = new Vector3(45f, 0, 0);

        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            currentAttackTimer = 0;
            attacksCount = 0;

            control.isAttacking = true;
            control.rigidBody.useGravity = false;
            control.rigidBody.velocity = control.characterSettings.launchedAttackPrepAbility.attackPrepMovementSpeed;
            control.rigidBody.AddForce(control.characterSettings.launchedAttackPrepAbility.attackPrepMovementForce);

            //var projectile = Singleton.Instance.spawnManager.SpawnThing<VFX_Type>(VFX_Type.VFX_FireBall, control.subComponentProcessor.attackSystem.projectileSpawnTransform.position, Quaternion.identity); OLD!
            var poolManager = Singleton.Instance.poolManager;
            var projectile = poolManager.GetObject<VFX_Type>(VFX_Type.VFX_FireBall, poolManager.vfxPoolDictionary, control.projectileSpawnTransform.position, Quaternion.identity);
            projectile.GetComponent<VFX>().SendProjectile_Fireball__TweenMove(control.projectileSpawnTransform.position, _finalProjectileRotation, control.characterSettings.launchedAttackPrepAbility.attackDamage);

        }
        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (attacksCount < ((int)stateInfo.normalizedTime))
            {
                attacksCount = (int)stateInfo.normalizedTime;
                //var projectile = Singleton.Instance.spawnManager.SpawnThing<VFX_Type>(VFX_Type.VFX_FireBall, control.subComponentProcessor.attackSystem.projectileSpawnTransform.position, Quaternion.identity); OLD!
                var poolManager = Singleton.Instance.poolManager;
                var projectile = poolManager.GetObject<VFX_Type>(VFX_Type.VFX_FireBall, poolManager.vfxPoolDictionary, control.projectileSpawnTransform.position, Quaternion.identity);
                projectile.GetComponent<VFX>().SendProjectile_Fireball__TweenMove(control.projectileSpawnTransform.position, _finalProjectileRotation, control.characterSettings.launchedAttackPrepAbility.attackDamage);
            }

            if (control.characterSettings.launchedAttackPrepAbility.useAnimationNormalizedTimeDuration)
            {
                if (stateInfo.normalizedTime >= control.characterSettings.launchedAttackPrepAbility.timesToRepeat_AttackPrep_State)
                {
                    control.isAttacking = false;
                    control.hasFinishedLaunchingTurn = true;
                }
            }
            else
            {
                currentAttackTimer += Time.deltaTime;
                if (currentAttackTimer >= control.characterSettings.launchedAttackPrepAbility.attackTimeDuration)
                {
                    control.isAttacking = false;
                    control.hasFinishedLaunchingTurn = true;
                }
            }
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.rigidBody.velocity = Vector3.zero;
        }
    }
}