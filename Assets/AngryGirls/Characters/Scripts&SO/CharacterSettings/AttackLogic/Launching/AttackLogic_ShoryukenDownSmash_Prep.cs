using UnityEngine;
using UnityEngine.VFX;

namespace Angry_Girls
{
    public class AttackLogic_ShoryukenDownSmash_Prep : AttackAbilityLogic
    {
        private GameObject _vfx;
        public override void OnStateEnter(CharacterControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.isAttacking = true;
            control.rigidBody.velocity = control.characterSettings.launchedAttackPrepAbility.attackPrepMovementSpeed;

            _vfx = CastFlameVFX(control);


        }
        public override void OnStateUpdate(CharacterControl control, Animator animator, AnimatorStateInfo stateInfo)
        {

        }

        public override void OnStateExit(CharacterControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            //_projectile.GetComponentInChildren<VisualEffect>().pause = true;
            _vfx.GetComponentInChildren<VisualEffect>().Stop();
        }

        private GameObject CastFlameVFX(CharacterControl control)
        {
            var vfx = Singleton.Instance.spawnManager.SpawnThing<VFX_Type>(VFX_Type.VFX_Flame2, control.subComponentProcessor.attackSystem.projectileSpawnTransform.position, Quaternion.identity);
            vfx.GetComponentInChildren<VisualEffect>().SetVector4("Color", control.subComponentProcessor.attackSystem.VFX_Color);
            vfx.transform.parent = control.transform;
            vfx.transform.position = control.subComponentProcessor.attackSystem.projectileSpawnTransform.position;
            vfx.GetComponent<VFX>().ApplyFlame(control.characterSettings.launchedAttackPrepAbility.attackDamage);

            return vfx;
        }
    }
}