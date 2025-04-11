using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_Alternate_Uppercut_Prep : AttackAbilityLogic
    {
        public AttackLogic_Alternate_Uppercut_Prep(AttackAbilityData attackAbilityData) : base(attackAbilityData) { }

        private GameObject _vFX;
        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            base.OnStateEnter(control, animator, stateInfo);
            _vFX = GameLoader.Instance.VFXManager.SpawnByProjectileAbility(control);
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.CharacterMovement.ResetVelocity();
            GameLoader.Instance.VFXManager.FadeOutAndDisposeVFX(_vFX, disposeDuration: 2f, fadeDuration: 3.5f);
        }
    }
}

