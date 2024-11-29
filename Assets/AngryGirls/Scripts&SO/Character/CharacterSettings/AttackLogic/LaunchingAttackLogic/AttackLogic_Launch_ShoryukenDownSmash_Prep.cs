using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_Launch_ShoryukenDownSmash_Prep : AttackAbilityLogic
    {
        private GameObject _vfx;
        float tempZlocation;
        
        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            tempZlocation = control.transform.position.z;
            control.isAttacking = true;
            control.rigidBody.velocity = control.characterSettings.AttackAbility_Launch.attackMovementSpeed;

            _vfx = GameLoader.Instance.VFXManager.SpawnVFX(control, control.characterSettings.AttackAbility_Launch.AttackVFX.GetComponent<VFX>().GetVFXType(), setAsOwner: true);
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.transform.position = new Vector3(0, control.transform.position.y, tempZlocation);

            foreach (var contact in control.boxColliderContacts)
            {
                var character = contact.otherCollider.gameObject.GetComponent<CControl>();
                if (character != null)
                {
                    //character.JostleFromEnemy(3);
                    control.transform.position = new Vector3(0, control.transform.position.y - 0.01f, tempZlocation);
                }
            }
            if (control.isGrounded)
            {                
                //control.subComponentMediator.TEMP_SetShorukenLandingState();
            }
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            GameLoader.Instance.VFXManager.FadeOutFlame_And_Dispose(_vfx.GetComponent<VFX>(), 2f, 3f);
        }
    }
}