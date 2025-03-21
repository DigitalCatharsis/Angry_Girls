using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_Launch_ShoryukenDownSmash_Prep : AttackAbilityLogic
    {
        private GameObject _vfx;
        //float tempZlocation;

        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.rigidBody.velocity = new Vector3(0, control.rigidBody.velocity.y, 0);            
            //tempZlocation = control.rigidBody.position.z;
            control.isAttacking = true;
            control.rigidBody.AddForce(control.characterSettings.AttackAbility_Launch.attackMovementSpeed, ForceMode.VelocityChange);

            _vfx = GameLoader.Instance.VFXManager.SpawnVFX(control, control.characterSettings.AttackAbility_Launch.AttackVFX.GetComponent<VFX>().GetVFXType(), setAsOwner: true);
            _vfx.GetComponent<VFX>().InitAndRunVFX_ByAbility(control.characterSettings.AttackAbility_Launch, control);
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            //control.rigidBody.position = new Vector3(0, control.rigidBody.position.y, tempZlocation);

            //if (control.boxColliderContacts != null)
            //{
            //    foreach (var contact in control.boxColliderContacts)
            //    {
            //        var character = contact.otherCollider.gameObject.GetComponent<CControl>();
            //        if (character != null)
            //        {
            //            //character.JostleFromEnemy(3);
            //            control.rigidBody.position = new Vector3(0, control.rigidBody.position.y - 0.01f, tempZlocation);
            //        }
            //    }
            //}
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            //TODO: fix
            GameLoader.Instance.VFXManager.FadeOutFlame_And_Dispose(_vfx.GetComponent<VFX>(), 2f, 3f);
        }
    }
}