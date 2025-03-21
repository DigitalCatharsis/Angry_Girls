using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_Launch_Uppercut_Prep : AttackAbilityLogic
    {
        public AttackLogic_Launch_Uppercut_Prep(AttackAbilityData attackAbilityData) : base(attackAbilityData) { }
        private GameObject _vfx;
        //float tempZlocation;

        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            base.OnStateEnter(control, animator, stateInfo);

            _vfx = GameLoader.Instance.VFXManager.SpawnVFX(control, control.characterSettings.AttackAbility_Launch.AttackVFX.GetComponent<VFX>().GetVFXType(), setAsOwner: true);
            _vfx.GetComponent<VFX>().InitAndRunVFX_ByAbility(control.characterSettings.AttackAbility_Launch, control);
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            //TODO: fix
            GameLoader.Instance.VFXManager.FadeOutFlame_And_Dispose(_vfx.GetComponent<VFX>(), 2f, 3f);
        }
    }
}