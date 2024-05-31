using UnityEngine;
using UnityEngine.VFX;

namespace Angry_Girls
{
    public class AttackLogic_ShoryukenDownSmash_Prep : AttackAbilityLogic
    {
        private GameObject _projectile;
        public override void OnStateEnter(CharacterControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.isAttacking = true;
            control.rigidBody.velocity = control.characterSettings.attackPrepAbility.attackPrepMovementSpeed;

            _projectile = Singleton.Instance.spawnManager.SpawnThing<VFX_Type>(VFX_Type.VFX_Flame2, control.subComponentProcessor.attackSystem.projectileSpawnTransform.position, Quaternion.identity);
            _projectile.GetComponentInChildren<VisualEffect>().SetVector4("Color", control.subComponentProcessor.attackSystem.VFX_Color);
            _projectile.transform.parent = control.transform;
            _projectile.transform.position = control.subComponentProcessor.attackSystem.projectileSpawnTransform.position;
            _projectile.GetComponent<VFX>().ApplyFlame(control.characterSettings.attackPrepAbility.attackDamage);
        }
        public override void OnStateUpdate(CharacterControl control, Animator animator, AnimatorStateInfo stateInfo)
        {

        }

        public override void OnStateExit(CharacterControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            //_projectile.GetComponentInChildren<VisualEffect>().pause = true;
            _projectile.GetComponentInChildren<VisualEffect>().Stop();
        }
    }
}