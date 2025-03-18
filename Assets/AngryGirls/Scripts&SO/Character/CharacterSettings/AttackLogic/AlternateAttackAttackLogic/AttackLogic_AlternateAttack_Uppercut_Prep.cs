using UnityEngine;
using UnityEngine.VFX;

namespace Angry_Girls
{
    public class AttackLogic_AlternateAttack_Uppercut_Prep : AttackAbilityLogic
    {
        public AttackLogic_AlternateAttack_Uppercut_Prep(AttackAbilityData attackAbilityData) : base(attackAbilityData) { }

        private GameObject _runningVFX;
        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            base.OnStateEnter(control, animator, stateInfo);
            _runningVFX = GameLoader.Instance.VFXManager.SpawnVFX(control, control.characterSettings.AttackAbility_Alternate.AttackVFX.GetComponent<VFX>().GetVFXType(), setAsOwner: true);

        }
        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.rigidBody.velocity = Vector3.zero;
            GameLoader.Instance.VFXManager.FadeOutFlame_And_Dispose(_runningVFX.GetComponent<VFX>(), 3.5f, 3.5f);
        }
    }
}

