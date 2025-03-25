using UnityEngine;
using UnityEngine.VFX;

namespace Angry_Girls
{
    public class AttackLogic_Alternate_Uppercut_Prep : AttackAbilityLogic
    {
        public AttackLogic_Alternate_Uppercut_Prep(AttackAbilityData attackAbilityData) : base(attackAbilityData) { }

        private GameObject _vFX;
        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            base.OnStateEnter(control, animator, stateInfo);
            _vFX = GameLoader.Instance.VFXManager.SpawnVFX(control, VFX_Type.VFX_Flame2, setAsOwner: true);
            _vFX.GetComponent<VFX>().InitAndRunVFX_ByAbility(control.characterSettings.AttackAbility_Alternate, control);
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.CharacterMovement.ResetVelocity();
            GameLoader.Instance.VFXManager.FadeOutFlame_And_Dispose(vFX: _vFX.GetComponent<VFX>(), disposeDuration: 2f, fadeDuration: 3.5f);
        }
    }
}

